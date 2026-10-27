<template>
  <!-- "synchronize outlines" on the Personalization settings group (legacy runOutlinesSynchronizationWidget) -->
  <VcWidget
    icon="lucide-refresh-cw"
    :title="$t('PERSONALIZATION.SYNC.TITLE')"
    :loading="busy"
    @click="run"
  />
</template>

<script setup lang="ts">
import { useBlade } from "@vc-shell/framework";
import { VcWidget } from "@vc-shell/framework/ui";
import { ref } from "vue";
import { http } from "../api";
import type { JobNotification } from "./useJob";

const { openBlade } = useBlade();
const busy = ref(false);

async function run() {
  busy.value = true;
  try {
    const notification = await http<JobNotification>("POST", "/api/personalization/outlines/synchronize", {});
    openBlade({ name: "PersonalizationOutlinesSync", options: { notification } });
  } finally {
    busy.value = false;
  }
}
</script>
