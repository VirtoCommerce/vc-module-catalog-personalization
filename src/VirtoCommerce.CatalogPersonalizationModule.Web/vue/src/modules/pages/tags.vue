<template>
  <VcBlade
    :title="$t('PERSONALIZATION.TAGS.TITLE')"
    :subtitle="options?.item.name"
    :toolbar-items="toolbar"
    :loading="loading"
    width="40%"
  >
    <VcContainer>
      <VcForm class="tags-form">
        <VcInput
          v-if="rows.length > 8"
          v-model="filter"
          :placeholder="$t('PERSONALIZATION.TAGS.FILTER')"
          clearable
        />
        <div
          v-for="row in visible"
          :key="row.key"
          class="personalization-tags__row"
        >
          <VcCheckbox
            :model-value="assigned.includes(row.tag)"
            @update:model-value="(v: boolean | boolean[]) => toggle(row.tag, !!v)"
          >
            {{ row.tag === "__any" ? $t("PERSONALIZATION.TAGS.ANY") : row.tag }}
          </VcCheckbox>
          <div class="tw-inline-flex">
            <VcStatus
              v-if="row.inherited"
              variant="info"
            >
              {{ $t(assigned.includes(row.tag) ? "PERSONALIZATION.TAGS.ALSO_INHERITED" : "PERSONALIZATION.TAGS.INHERITED") }}
            </VcStatus>
          </div>
        </div>
        <div
          v-if="!rows.length"
          class="tags-form__hint"
        >
          {{ $t("PERSONALIZATION.TAGS.EMPTY") }}
        </div>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcCheckbox, VcContainer, VcForm, VcInput, VcStatus } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { http, settingValues } from "../api";

// Catalog personalization (VirtoCommerce.CatalogPersonalization) tag-list for a product or category:
// tags come from the Customer.MemberGroups setting; inherited tags are shown but never saved.
// Not routable: the product/category arrives only through options.
defineBlade({ url: "/personalization-tags", name: "PersonalizationTags", routable: false });

interface TaggedItem {
  id?: string;
  label?: string;
  entityId?: string;
  entityType?: string;
  tags?: string[];
  inheritedTags?: string[];
}

const { t } = useI18n({ useScope: "global" });
const { options, openBlade, callParent } = useBlade<{ item: { id?: string; name?: string }; entityType: "Product" | "Category" }>();

const tagged = ref<TaggedItem>({});
const dictionary = ref<string[]>([]);
const assigned = ref<string[]>([]);
const loading = ref(false);
const filter = ref("");
const lower = (s: string) => s.toLowerCase();

const rows = computed(() => {
  const inherited = new Set((tagged.value.inheritedTags ?? []).map(lower));
  const seen = new Map<string, string>();
  for (const tag of [...(tagged.value.tags ?? []), ...(tagged.value.inheritedTags ?? []), ...dictionary.value])
    if (!seen.has(lower(tag))) seen.set(lower(tag), tag);
  return [...seen.values()]
    .map((tag) => ({ tag, key: lower(tag), inherited: inherited.has(lower(tag)) }))
    .sort((a, b) => Number(isOn(b)) - Number(isOn(a)) || a.tag.localeCompare(b.tag));
});
const isOn = (r: { tag: string; inherited: boolean }) => r.inherited || assigned.value.includes(r.tag);
const visible = computed(() => rows.value.filter((r) => !filter.value || r.key.includes(lower(filter.value))));
const modified = computed(() => [...assigned.value].sort().join("|") !== [...(tagged.value.tags ?? [])].sort().join("|"));

function toggle(tag: string, on: boolean) {
  assigned.value = on ? [...assigned.value, tag] : assigned.value.filter((x) => x !== tag);
}

async function load() {
  if (!options.value?.item.id) return;
  loading.value = true;
  try {
    tagged.value = (await http<TaggedItem | undefined>("GET", `/api/personalization/taggeditem/${options.value.item.id}`)) ?? {};
    assigned.value = [...(tagged.value.tags ?? [])];
  } finally {
    loading.value = false;
  }
}

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "save",
    title: t("PERSONALIZATION.SAVE"),
    icon: "lucide-save",
    permissions: "personalization:update",
    disabled: !modified.value || !options.value,
    async clickHandler() {
      if (!options.value) return;
      loading.value = true;
      try {
        await http("PUT", "/api/personalization/taggeditem", {
          ...tagged.value,
          entityId: options.value.item.id,
          label: options.value.item.name,
          entityType: options.value.entityType,
          tags: assigned.value,
        });
      } finally {
        loading.value = false;
      }
      await load();
      callParent("reload");
    },
  },
  {
    id: "reset",
    title: t("PERSONALIZATION.RESET"),
    icon: "lucide-undo-2",
    disabled: !modified.value,
    clickHandler: () => (assigned.value = [...(tagged.value.tags ?? [])]),
  },
  {
    // the tag dictionary is the Customer.MemberGroups setting: edited in the Platform plugin's dictionary blade
    id: "dictionary",
    title: t("PERSONALIZATION.TAGS.EDIT_DICTIONARY"),
    icon: "lucide-book-open",
    permissions: "platform:setting:update",
    clickHandler: () =>
      openBlade({
        name: "PlatformSettingDictionary",
        param: "Customer.MemberGroups",
        onClose: async () => (dictionary.value = await settingValues("Customer.MemberGroups")),
      }),
  },
]);

onMounted(async () => {
  await Promise.all([settingValues("Customer.MemberGroups").then((x) => (dictionary.value = x)), load()]);
});
</script>

<style lang="scss">
.personalization-tags__row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 6px 0;
  border-bottom: 1px solid var(--neutrals-100);
}
</style>
