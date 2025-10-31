
Patched for configurable Add/Edit cards with a single ConfigPanelComponent.

Key files changed/added:
- api/reportForm.ts — mock AJAX for createCard/updateCard/refetchCardData + types
- components/ConfigPanelComponent.vue — modal panel, supports add/edit, dynamic sub-panels per type
- components/BaseCardComponent.vue — standardized header with 修改/刪除 emits
- components/panels/* — simple config stubs (used by ConfigPanelComponent)
- components/cardInfos/MapHeatmapCardComponent.vue — demo-only heatmap dots
- pages/IndexView.vue — wires everything: "新增" opens panel; Confirm adds or updates only the targeted card via ajax; Cancel closes panel
