<template>
  <VcBlade
    :title="$t('PERSONALIZATION.SYNC.TITLE')"
    :toolbar-items="toolbar"
    width="40%"
  >
    <VcContainer>
      <VcForm class="tags-form">
        <JobProgress
          :job="job"
          :finished="finished"
          :percent="percent"
          prefix="PERSONALIZATION"
        />
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcContainer, VcForm } from "@vc-shell/framework/ui";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import { http } from "../api";
import JobProgress from "../components/JobProgress.vue";
import { useJob, type JobNotification } from "../components/useJob";

// legacy outlines-synchronization-progress (catalog personalization): progress and cancel.
// Not routable: the job's notification arrives only through options.
defineBlade({ url: "/personalization-outlines-sync", name: "PersonalizationOutlinesSync", routable: false });

const { t } = useI18n({ useScope: "global" });
const { options } = useBlade<{ notification: JobNotification }>();
const { job, finished, percent, follow } = useJob();
if (options.value?.notification) follow(options.value.notification);

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "cancel",
    title: t("PERSONALIZATION.SYNC.CANCEL"),
    icon: "lucide-circle-stop",
    disabled: finished.value || !job.value?.jobId,
    // the legacy cancel was broken (undefined taskApi); this calls the module's endpoint
    clickHandler: () => http("POST", "/api/personalization/outlines/synchronization/cancel", { jobId: job.value!.jobId }),
  },
]);
</script>
