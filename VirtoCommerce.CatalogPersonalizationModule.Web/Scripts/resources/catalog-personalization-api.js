angular.module('virtoCommerce.catalogPersonalizationModule')
    .factory('virtoCommerce.vatalogPersonalizationModule.catalogPersonalization', ['$resource', function ($resource) {
        return $resource('api/personalization');
}]);
