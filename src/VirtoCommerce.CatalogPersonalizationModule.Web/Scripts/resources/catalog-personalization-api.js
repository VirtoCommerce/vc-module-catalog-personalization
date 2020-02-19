angular.module('virtoCommerce.catalogPersonalizationModule')
    .factory('virtoCommerce.personalizationModule.personalizationApi', ['$resource', function ($resource) {
        return $resource('api/personalization', {}, {
            taggedItem: { url: 'api/personalization/taggeditem/:id', method: 'GET' },
            tagsCount: { url: 'api/personalization/taggeditem/:id/tags/count', method: 'GET' },
            update: { url: 'api/personalization/taggeditem', method: 'PUT' },
            synchronizeOutlines: { url: 'api/personalization/outlines/synchronize', method: 'POST' },
            cancelSynchronization: { url: 'api/personalization/outlines/synchronization/cancel', method: 'POST' }            
        });
}]);
