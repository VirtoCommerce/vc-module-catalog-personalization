angular.module('virtoCommerce.catalogPersonalizationModule')
    .controller('virtoCommerce.catalogPersonalizationModule.blade1Controller', ['$scope', 'virtoCommerce.vatalogPersonalizationModule.catalogPersonalization', function ($scope, catalogPersonalizationApi) {
        var blade = $scope.blade;
        blade.title = 'VirtoCommerce.CatalogPersonalizationModule.Web';

        blade.refresh = function () {
            catalogPersonalizationApi.get(function (data) {
                blade.data = data.result;
                blade.isLoading = false;
            });
        }

        blade.refresh();
    }]);
