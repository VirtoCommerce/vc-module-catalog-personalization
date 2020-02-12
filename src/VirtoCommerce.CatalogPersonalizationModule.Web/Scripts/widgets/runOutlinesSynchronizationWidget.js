angular.module('virtoCommerce.catalogPersonalizationModule')
    .controller('virtoCommerce.catalogPersonalizationModule.runOutlinesSynchronizationWidgetController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.personalizationModule.personalizationApi',
        function ($scope, bladeNavigationService, personalizationApi) {
            var blade = $scope.blade;
            blade.isRunning = false;

            $scope.runSynchronization = function () {

                personalizationApi.synchronizeOutlines({}, function (notification) {
                    if (!blade.isRunning) {
                        blade.isRunning = true;

                        var progressBlade = {
                            id: 'outlinesSynchronizationProgress',
                            controller: 'virtoCommerce.catalogPersonalizationModule.outlinesSynchronizationProgressController',
                            template: 'Modules/$(VirtoCommerce.CatalogPersonalization)/Scripts/blades/outlines-synchronization-progress.tpl.html',
                            notification: notification,
                            onCompleted: function () {
                                blade.isRunning = false;
                            }
                        };

                        bladeNavigationService.showBlade(progressBlade, blade);
                    }
                });
            };

        }]);
