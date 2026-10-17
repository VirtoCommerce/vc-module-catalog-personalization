import * as blades from "./pages";
import * as locales from "./locales";
import { defineAppModule, registerExternalWidget } from "@vc-shell/framework";
import { defineComponent, h, markRaw } from "vue";
import TagsWidget from "./components/TagsWidget.vue";
import OutlinesSyncWidget from "./components/OutlinesSyncWidget.vue";
import "./styles.scss";

// Widgets the legacy module registered into the Catalog blades ("itemDetail", "categoryDetail")
for (const [blade, entityType] of [
  ["CatalogProductDetails", "Product"],
  ["CatalogCategoryDetails", "Category"],
] as const) {
  registerExternalWidget({
    id: `PersonalizationTags${entityType}`,
    title: "Tags",
    component: markRaw(defineComponent({ name: `PersonalizationTags${entityType}Widget`, setup: () => () => h(TagsWidget, { entityType }) })),
    targetBlades: [blade],
  });
}

// "Synchronize outlines" on the Settings blade while the Personalization group is chosen in its tree
// (legacy settingsDetail widget); the blade keeps the chosen group in its query (?group=Personalization|General)
registerExternalWidget({
  id: "PersonalizationOutlinesSync",
  title: "Synchronize outlines",
  component: markRaw(OutlinesSyncWidget),
  targetBlades: ["PlatformSettings"],
  isVisible: (blade) => /^Personalization(\||$)/.test(blade?.query?.group ?? ""),
});

export default defineAppModule({
  blades,
  locales,
});

export * from "./pages";
