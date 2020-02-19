angular.module('virtoCommerce.catalogPersonalizationModule')
    .controller('virtoCommerce.catalogPersonalizationModule.tagsWidgetController', ['$scope', '$filter', 'platformWebApp.bladeNavigationService', '$state', 'virtoCommerce.personalizationModule.personalizationApi',
        function ($scope, $filter, bladeNavigationService, $state, personalizationApi) {
            var blade = $scope.blade;
            blade.itemData = {};
            $scope.assignedTagsCount = 0;
            $scope.inheritedTagsCount = 0;
            function refresh() {
                personalizationApi.taggedItem({ id: blade.itemData.id },
                    function (result) {
                        $scope.loading = false;
                        $scope.assignedTagsCount = result.tags ? result.tags.length : 0;
                        $scope.inheritedTagsCount = result.inheritedTags ? result.inheritedTags.length : 0;
                    });
            }

            $scope.openBlade = function () {
                if ($scope.loading)
                    return;

                //Catalog or category
                if (blade.origEntity) {
                    blade.itemData.name = blade.origEntity.name;
                    blade.itemData.type = "Category";
                }
                //Product
                else if (blade.item) {
                    blade.itemData.name = blade.item.name;
                    blade.itemData.type = "Product";
                }

                var taggetItemBlade = {
                    id: "tagList",
                    item: blade.itemData,
                    parentWidgetRefresh: refresh,
                    title: blade.itemData.name,
                    subtitle: blade.itemData.type,
                    controller: 'virtoCommerce.catalogPersonalizationModule.tagListController',
                    template: 'Modules/$(VirtoCommerce.CatalogPersonalization)/Scripts/blades/tag-list.tpl.html'
                };
                bladeNavigationService.showBlade(taggetItemBlade, blade);
            };

            blade.itemData.id = blade.currentEntityId;

            refresh();
        }]);
