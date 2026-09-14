angular.module('virtoCommerce.catalogPersonalizationModule')
    .controller('virtoCommerce.catalogPersonalizationModule.tagListController', ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.settings', 'virtoCommerce.personalizationModule.personalizationApi', '$translate',
        function ($scope, bladeNavigationService, settings, personalizationApi, $translate) {
            var blade = $scope.blade;

            var settingKey = 'Customer.MemberGroups';
            var anyTag = '__any';
            // Below this many groups the filter box costs more space than it saves.
            var filterThreshold = 8;

            blade.updatePermission = 'personalization:update';
            blade.tagsDictionary = [];
            blade.rows = [];
            blade.visibleRows = [];
            blade.filter = '';
            blade.anyLabel = anyTag;
            blade.stats = { direct: 0, inherited: 0, total: 0 };
            blade.summaryText = '';
            blade.noMatchText = '';
            blade.origEntity = undefined;
            blade.currentEntity = undefined;

            $translate('personalization.tags.__any').then(function (label) {
                blade.anyLabel = label;
                var anyRow = _.find(blade.rows, function (x) { return x.isAny; });
                if (anyRow) {
                    anyRow.displayName = label;
                    sortRows();
                    applyFilter();
                }
            });

            blade.refresh = function () {
                blade.isLoading = true;
                settings.getValues({ id: settingKey }, function (tagsDictionary) {
                    blade.tagsDictionary = tagsDictionary;
                    loadTaggedItem();
                });
            };

            function loadTaggedItem() {
                personalizationApi.taggedItem({ id: blade.item.id }, function (result) {
                    blade.currentEntity = result || {};
                    blade.currentEntity.tags = blade.currentEntity.tags || [];
                    blade.currentEntity.inheritedTags = blade.currentEntity.inheritedTags || [];
                    blade.origEntity = angular.copy(blade.currentEntity);
                    buildRows();
                    blade.isLoading = false;
                }, function (error) {
                    blade.isLoading = false;
                    bladeNavigationService.setError('Error ' + error.status, blade);
                });
            }

            // Collapses the dictionary, the directly assigned tags and the inherited tags into one
            // row per group, so a group that is both assigned and inherited is shown once.
            function buildRows() {
                var rowsByKey = {};

                function ensure(value) {
                    var key = value.toLowerCase();
                    if (!rowsByKey[key]) {
                        rowsByKey[key] = {
                            value: value,
                            displayName: value,
                            isAssigned: false,
                            isInherited: false,
                            inDictionary: false,
                            isAny: value === anyTag
                        };
                    }
                    return rowsByKey[key];
                }

                _.each(blade.tagsDictionary, function (x) { ensure(x).inDictionary = true; });
                _.each(blade.currentEntity.tags, function (x) { ensure(x).isAssigned = true; });
                _.each(blade.currentEntity.inheritedTags, function (x) { ensure(x).isInherited = true; });

                _.each(rowsByKey, function (row) {
                    row.isOrphan = !row.inDictionary && !row.isAny;
                    if (row.isAny) {
                        row.displayName = blade.anyLabel;
                    }
                    // Rank is fixed from the loaded state so that ticking a box never reorders the list
                    // under the pointer. It is recomputed only when the blade reloads from the server.
                    row.sortRank = (row.isAssigned || row.isInherited) ? '0' : '1';
                });

                blade.rows = _.values(rowsByKey);
                sortRows();
                updateStats();
                applyFilter();
            }

            function sortRows() {
                blade.rows = _.sortBy(blade.rows, function (row) {
                    return row.sortRank + '|' + row.displayName.toLowerCase();
                });
            }

            function applyFilter() {
                var term = (blade.filter || '').trim().toLowerCase();
                blade.visibleRows = !term
                    ? blade.rows
                    : _.filter(blade.rows, function (row) {
                        return row.displayName.toLowerCase().indexOf(term) >= 0;
                    });
                blade.noMatchText = $translate.instant('personalization.blades.tag-list.labels.no-matches', { term: blade.filter });
            }

            $scope.$watch('blade.filter', applyFilter);

            blade.showFilter = function () {
                return blade.rows.length > filterThreshold;
            };

            // The checkbox governs the direct assignment only. Inheritance is granted by a parent
            // category and is shown as a badge, so every row stays toggleable in both directions.
            blade.toggle = function (row) {
                row.isAssigned = !row.isAssigned;
                updateStats();
            };

            // Rows are reachable with Tab and toggled with Space, per the ARIA checkbox pattern
            // (Enter is deliberately not bound - it belongs to buttons, not checkboxes).
            // Without preventDefault the blade scrolls a page down on every Space.
            blade.onRowKeydown = function (event, row) {
                var isSpace = event.key === ' ' || event.key === 'Spacebar' ||
                    event.which === 32 || event.keyCode === 32;
                if (isSpace) {
                    event.preventDefault();
                    blade.toggle(row);
                }
            };

            blade.clearFilter = function () {
                blade.filter = '';
            };

            function updateStats() {
                blade.stats.direct = _.filter(blade.rows, function (x) { return x.isAssigned; }).length;
                blade.stats.inherited = _.filter(blade.rows, function (x) { return x.isInherited; }).length;
                blade.stats.total = _.filter(blade.rows, function (x) { return x.isAssigned || x.isInherited; }).length;
                // Resolved here rather than through the translate filter: passing an object literal to a
                // non-stateful filter makes every digest dirty.
                blade.summaryText = $translate.instant('personalization.blades.tag-list.labels.summary', blade.stats);
            }

            function assignedTags() {
                return _.map(_.filter(blade.rows, function (row) { return row.isAssigned; }),
                    function (row) { return row.value; });
            }

            function isDirty() {
                if (!blade.origEntity) {
                    return false;
                }
                var current = assignedTags().sort();
                var original = (blade.origEntity.tags || []).slice().sort();
                return !angular.equals(current, original);
            }

            $scope.editTagsDictionary = function () {
                var editTagsDictionaryBlade = {
                    id: "settingDetailChild",
                    currentEntityId: settingKey,
                    isApiSave: true,
                    controller: 'platformWebApp.settingDictionaryController',
                    template: '$(Platform)/Scripts/app/settings/blades/setting-dictionary.tpl.html',
                    onClose: function (doCloseBlade) {
                        doCloseBlade();
                        // Keep the pending edits, only pick up dictionary additions and removals.
                        var pending = assignedTags();
                        blade.isLoading = true;
                        settings.getValues({ id: settingKey }, function (tagsDictionary) {
                            blade.tagsDictionary = tagsDictionary;
                            blade.currentEntity.tags = pending;
                            buildRows();
                            blade.isLoading = false;
                        });
                    }
                };
                bladeNavigationService.showBlade(editTagsDictionaryBlade, blade);
            };

            $scope.saveChanges = function () {
                blade.isLoading = true;

                blade.currentEntity.entityId = blade.item.id;
                blade.currentEntity.label = blade.item.name;
                blade.currentEntity.entityType = blade.item.type;
                blade.currentEntity.tags = assignedTags();

                // The endpoint answers 204 No Content, so the saved state has to be read back.
                personalizationApi.update(blade.currentEntity, function () {
                    loadTaggedItem();
                    if (angular.isFunction(blade.parentWidgetRefresh)) {
                        blade.parentWidgetRefresh();
                    }
                }, function (error) {
                    blade.isLoading = false;
                    bladeNavigationService.setError('Error ' + error.status, blade);
                });
            };

            blade.toolbarCommands = [
                {
                    name: "platform.commands.save",
                    icon: 'fas fa-save',
                    executeMethod: $scope.saveChanges,
                    canExecuteMethod: isDirty,
                    permission: blade.updatePermission
                },
                {
                    name: "platform.commands.reset",
                    icon: 'fa fa-undo',
                    executeMethod: function () {
                        blade.filter = '';
                        blade.currentEntity = angular.copy(blade.origEntity);
                        buildRows();
                    },
                    canExecuteMethod: isDirty
                }
            ];

            blade.refresh();
        }]);
