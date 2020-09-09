var moduleName = "virtoCommerce.catalogPersonalizationModule";

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
.config(['$stateProvider', '$urlRouterProvider',
    function ($stateProvider, $urlRouterProvider) {
        $stateProvider.state('workspace.catalogPersonalizationModule', {
            url: '/catalogPersonalization',
            templateUrl: '$(Platform)/Scripts/common/templates/home.tpl.html',
            controller: [
                '$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
                }
            ]
        });
    }
])
.run(['$rootScope', 'platformWebApp.mainMenuService', 'platformWebApp.widgetService', '$state',
    function ($rootScope, mainMenuService, widgetService, $state) {

        //Register tags widget
        var tagsWidget = {
            controller: 'virtoCommerce.catalogPersonalizationModule.tagsWidgetController',
            size: [1, 1],
            template: 'Modules/$(VirtoCommerce.CatalogPersonalization)/Scripts/widgets/tagsWidget.tpl.html',
            isVisible: function (blade) { return !blade.isNew; }
        };

        //Category properties blade
        widgetService.registerWidget(tagsWidget, 'categoryDetail');
        
        //Product details blade
        widgetService.registerWidget(tagsWidget, 'itemDetail');

        // Register manual outlines synchronization widget in module settings
        widgetService.registerWidget({
            isVisible: function (blade) {
                return blade.data && _.some(blade.data, function (item) { return item.groupName === "Personalization|General"; });
            },
            controller: 'virtoCommerce.catalogPersonalizationModule.runOutlinesSynchronizationWidgetController',
            template: 'Modules/$(VirtoCommerce.CatalogPersonalization)/Scripts/widgets/runOutlinesSynchronizationWidget.tpl.html'
        },
        'settingsDetail');
    }
]);
