angular.module('virtoCommerce.catalogPersonalizationModule')
    .controller('virtoCommerce.catalogPersonalizationModule.tagListController', ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.ui-grid.extension', 'platformWebApp.dialogService', 'platformWebApp.settings', 'virtoCommerce.personalizationModule.personalizationApi', '$timeout', '$translate',
        function ($scope, bladeNavigationService, gridOptionExtension, dialogService, settings, personalizationApi, $timeout, $translate) {
            var blade = $scope.blade;
            blade.tagsDictionary = [];
            blade.origEntity = undefined;
            blade.currentEntity = undefined;

            var settingKey = 'Customer.MemberGroups';
            blade.updatePermission = 'personalization:update';
            
            settings.getValues({ id: settingKey }, function (tagsDictionary) {
                blade.tagsDictionary = tagsDictionary;

                personalizationApi.taggedItem({ id: blade.item.id },
                    function(result) {
                        blade.currentEntity = result.taggedItem || {};
                        blade.currentEntity.tags = blade.currentEntity.tags || [];
                        blade.currentEntity.inheritedTags = blade.currentEntity.inheritedTags || [];

                        _.each(blade.currentEntity.inheritedTags, function(tag, idx, list) {
                            if (tag === '__any') {
                                $translate('personalization.tags.__any').then(function (result) {
                                    list[idx] = result;
                                });
                            }
                        });

                    blade.origEntity = angular.copy(blade.currentEntity);
                    blade.isLoading = false;
                });
            });

            $scope.editTagsDictionary = function () {
                var editTagsDictionaryBlade = {
                    id: "settingDetailChild",
                    currentEntityId: settingKey,
                    isApiSave: true,
                    controller: 'platformWebApp.settingDictionaryController',
                    template: '$(Platform)/Scripts/app/settings/blades/setting-dictionary.tpl.html',
                    onClose: function (doCloseBlade) {
                        doCloseBlade();
                        blade.isLoading = true;
                        settings.getValues({ id: settingKey }, function (tagsDictionary) {
                            blade.tagsDictionary = tagsDictionary;
                            blade.availableTags = _.filter(tagsDictionary, function (x) {
                                return _.all(blade.currentEntity.tags, function (curr) { return curr !== x; });
                            });
                            blade.isLoading = false;
                        });
                    }
                };
                bladeNavigationService.showBlade(editTagsDictionaryBlade, blade);
            }
            
            blade.assignTag = function (selectedTag) {
                blade.currentEntity.tags.push(selectedTag);
                blade.selectedTag = undefined;
            }

            function canSave() {
                return isDirty() && blade.hasUpdatePermission();
            }

            function isDirty() {
                return !angular.equals(blade.currentEntity, blade.origEntity);
            }

            function isItemsChecked() {
                return _.any(blade.assignedTags, function (x) { return x.$selected; });
            }

            $scope.saveChanges = function () {
                blade.isLoading = true;

                blade.currentEntity.entityId = blade.item.id;
                blade.currentEntity.label = blade.item.name;
                blade.currentEntity.entityType = blade.item.type;

                personalizationApi.update(blade.currentEntity,
                    function (result) {
                        blade.currentEntity = result;
                        blade.origEntity = angular.copy(blade.currentEntity);
                        blade.isLoading = false;
                    },
                    function (error) {
                        bladeNavigationService.setError('Error ' + error.status, $scope.blade);
                    }
                );
            }

            function deleteChecked() {
                _.each(blade.assignedTags.slice(), function (x) {
                    if (x.$selected) {
                        blade.currentEntity.tags.splice(blade.currentEntity.tags.indexOf(x.value), 1);
                    }
                });
            }

            blade.toolbarCommands = [
                {
                    name: "platform.commands.save",
                    icon: 'fa fa-save',
                    executeMethod: $scope.saveChanges,
                    canExecuteMethod: canSave,
                    permission: blade.updatePermission
                },
                {
                    name: "platform.commands.reset",
                    icon: 'fa fa-undo',
                    executeMethod: function () {
                        //Reset assigned tags and availableTags
                        angular.copy(blade.origEntity, blade.currentEntity);
                        //Unselect selected
                        blade.selectedTag = undefined;
                        _.each(blade.assignedTags, function (x) {
                            if (x.$selected) {
                                x.$selected = false;
                            }
                        });
                    },
                    canExecuteMethod: isDirty,
                },
                {
                    name: "platform.commands.delete",
                    icon: 'fa fa-trash-o',
                    executeMethod: deleteChecked,
                    canExecuteMethod: isItemsChecked
                }
            ];

            $scope.$watchCollection('blade.currentEntity.tags', function (tags) {
                if (!tags) {
                    blade.assignedTags = [];
                    blade.availableTags = blade.tagsDictionary;
                }
                else {
                    blade.assignedTags = _.map(tags, function (tag) { return { value: tag }; });
                    blade.availableTags = _.filter(blade.tagsDictionary, function (x) {
                        return _.all(tags, function (curr) { return curr !== x; });
                    });
                }
            });
        }]);
