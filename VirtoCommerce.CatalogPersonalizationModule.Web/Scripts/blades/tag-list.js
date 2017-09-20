angular.module('virtoCommerce.catalogPersonalizationModule')
    .controller('virtoCommerce.catalogPersonalizationModule.tagListController', ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.ui-grid.extension', 'platformWebApp.dialogService', 'platformWebApp.settings', 'virtoCommerce.personalizationModule.personalizationApi', '$timeout',
        function ($scope, bladeNavigationService, gridOptionExtension, dialogService, settings, personalizationApi, $timeout) {
            var blade = $scope.blade;
            blade.allTags = [];
            blade.assignedTags = [];
            blade.origAssignedTags = [];
            blade.availableTags = [];
            blade.selectedTag = undefined;

            var settingKey = 'Personalization.Tags';
            var moduleId = 'VirtoCommerce.CatalogPersonalization';
            blade.updatePermission = 'personalization:update';
            
            settings.getValues({ id: settingKey }, function (allTags) {
                blade.allTags = allTags;

                personalizationApi.taggedItem({ id: blade.item.id }, function (result) {
                    blade.taggedItem = result.taggedItem || {};
                    var itemTags = [];

                    if (blade.taggedItem && blade.taggedItem.tags) {
                        itemTags = blade.taggedItem.tags;
                        blade.assignedTags = blade.convertToTagObjects(itemTags);
                        blade.origAssignedTags = angular.copy(blade.assignedTags);
                    }

                    blade.availableTags = _.filter(blade.allTags, function (x) {
                        return _.all(itemTags, function (curr) { return curr !== x; });
                    });

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
                        settings.getValues({ id: settingKey }, function (allTags) {
                            blade.allTags = allTags;
                            blade.availableTags = _.filter(allTags, function (x) {
                                return _.all(blade.assignedTags, function (curr) { return curr.value !== x; });
                            });
                            blade.isLoading = false;
                        });
                    }
                };
                bladeNavigationService.showBlade(editTagsDictionaryBlade, blade);
            }

            blade.convertToTagObjects = function (tags) {
                var tagsList = [];

                _.each(tags, function (tag) {
                    var tagObj = blade.getTagObject(tag);
                    tagsList.push(tagObj);
                });

                return tagsList;
            }

            blade.getTagObject = function (tag) {
                var tagObj = {
                    value: tag
                };
                return tagObj;
            }
            
            blade.assignTag = function (selectedTag) {
                var tagObj = blade.getTagObject(selectedTag);
                blade.assignedTags.push(tagObj);

                blade.availableTags = _.without(blade.availableTags, selectedTag);
                blade.selectedTag = undefined;
            }

            function canSave() {
                return isDirty() && blade.hasUpdatePermission();
            }

            function isDirty() {
                return !angular.equals(blade.assignedTags, blade.origAssignedTags);
            }

            function isItemsChecked() {
                return _.any(blade.assignedTags, function (x) { return x.$selected; });
            }

            $scope.saveChanges = function () {
                blade.isLoading = true;

                blade.taggedItem.entityId = blade.item.id;
                blade.taggedItem.label = blade.item.name;
                blade.taggedItem.entityType = blade.item.type;
                blade.taggedItem.tags = _.map(blade.assignedTags, function (tagObj) { return tagObj.value; });

                personalizationApi.update(blade.taggedItem,
                    function (result) {
                        blade.origAssignedTags = angular.copy(blade.assignedTags);
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
                        blade.assignedTags.splice(blade.assignedTags.indexOf(x), 1);
                    }
                });

                blade.availableTags = _.filter(blade.allTags, function (x) {
                    return _.all(blade.assignedTags, function (curr) { return curr !== x; });
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
                        //Reset assigned tags
                        angular.copy(blade.origAssignedTags, blade.assignedTags);
                        //Reset availableTags
                        blade.availableTags = _.filter(blade.allTags, function (x) {
                            return _.all(blade.assignedTags, function (curr) { return curr !== x; });
                        });
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
        }]);
