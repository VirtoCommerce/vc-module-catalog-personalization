var moduleName = "virtoCommerce.catalogPersonalizationModule";

if (AppDependencies != undefined) {
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
                    //var newBlade = {
                    //    id: 'blade1',
                    //    controller: 'virtoCommerce.catalogPersonalizationModule.blade1Controller',
                    //    template: 'Modules/$(VirtoCommerce.CatalogPersonalization)/Scripts/blades/helloWorld_blade1.tpl.html',
                    //    isClosingDisabled: true
                    //};
                    //bladeNavigationService.showBlade(newBlade);
                }
            ]
        });
    }
])
.run(['$rootScope', 'platformWebApp.mainMenuService', 'platformWebApp.widgetService', '$state',
    function ($rootScope, mainMenuService, widgetService, $state) {
        //Register module in main menu
        //var menuItem = {
        //    path: 'browse/catalogPersonalization',
        //    icon: 'fa fa-cube',
        //    title: 'personalization.main-menu-title',
        //    priority: 100,
        //    action: function () { $state.go('workspace.catalogPersonalizationModule') }
        //};
        //mainMenuService.addMenuItem(menuItem);

        //Register tags widget
        var tagsWidget = {
            isVisible: true,
            controller: 'virtoCommerce.catalogPersonalizationModule.tagsWidgetController',
            size: [1, 1],
            template: 'Modules/$(VirtoCommerce.CatalogPersonalization)/Scripts/widgets/tagsWidget.tpl.html'
        };

        //Catalog properties blade
        widgetService.registerWidget(tagsWidget, 'catalogDetail');

        //Category properties blade
        widgetService.registerWidget(tagsWidget, 'categoryDetail');
        
        //Product details blade
        widgetService.registerWidget(tagsWidget, 'itemDetail');
    }
]);
