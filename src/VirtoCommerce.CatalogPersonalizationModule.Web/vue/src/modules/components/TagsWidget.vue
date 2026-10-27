<template>
  <!-- Tags of a product or category (legacy tagsWidget on itemDetail / categoryDetail): count, opens the tag list -->
  <VcWidget
    icon="lucide-tags"
    :title="$t('PERSONALIZATION.TAGS.TITLE')"
    :value="count"
    :disabled="!entity?.id"
    @click="open"
  />
</template>

<script setup lang="ts">
import { injectBladeContext, useBlade } from "@vc-shell/framework";
import { VcWidget } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { http } from "../api";

const props = defineProps<{ entityType: "Product" | "Category" }>();

// the product/category shared by the Catalog blade via defineBladeContext({ item })
const ctx = injectBladeContext();
const entity = computed(() => ctx.value.item as { id?: string; name?: string } | undefined);
const { openBlade } = useBlade();
const count = ref<number | string>("…");

async function refresh() {
  if (!entity.value?.id) return;
  // assigned + inherited tags
  count.value = Number(await http("GET", `/api/personalization/taggeditem/${entity.value.id}/tags/count`).catch(() => 0)) || 0;
}
watch(() => entity.value?.id, refresh, { immediate: true });

function open() {
  if (!entity.value?.id) return;
  openBlade({
    name: "PersonalizationTags",
    options: { item: { id: entity.value.id, name: entity.value.name }, entityType: props.entityType },
    onClose: refresh,
  });
}
</script>
