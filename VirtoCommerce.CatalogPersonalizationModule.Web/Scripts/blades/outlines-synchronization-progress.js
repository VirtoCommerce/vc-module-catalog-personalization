angular.module('virtoCommerce.catalogPersonalizationModule')
    .controller('virtoCommerce.catalogPersonalizationModule.outlinesSynchronizationProgressController', ['$scope', 'virtoCommerce.personalizationModule.personalizationApi', function ($scope, personalizationApi) {
        var blade = $scope.blade;

        $scope.$on("new-notification-event", function (event, notification) {
            if (blade.notification && notification.id == blade.notification.id) {
                angular.copy(notification, blade.notification);
            }

            if (blade.notification.finished) {
                if (blade.onCompleted) {
                    blade.onCompleted();
                }
            }
        });

        blade.toolbarCommands = [{
            name: 'platform.commands.cancel',
            icon: 'fa fa-times',
            canExecuteMethod: function () {
                return blade.notification && !blade.notification.finished;
            },
            executeMethod: function () {
                taskApi.cancelSynchronization({ jobId: blade.notification.jobId });
            }
        }];

        blade.title = blade.notification.title;
        blade.headIcon = 'fa-file-text';
        blade.isLoading = false;
    }]);
