// Core Javascript file for GSoft.Dynamite.Client

// Chrome-JS intermitted crash fix (otherwise the ribbon breaks in Chrome 9 times out of 10)
if (window.chrome) {
    window[addEventListener ? 'addEventListener' : 'attachEvent'](addEventListener ? 'load' : 'onload', function () {
        try {
            if (_spBodyOnLoadWrapper) {
                _spBodyOnLoadWrapper();
            }
        } catch (e) {
            // console.log("Error while forcing execution of _spBodyOnLoadWrapper " + e);
        }
    });
}

moment.lang('fr', {
    longDateFormat: {
        // Use the Microsoft official fr-CA regional setting date format, for consistency between presentation and SharePoint backend format
        L: "YYYY-MM-DD"
    }
});


moment.lang('en', {
    longDateFormat: {
        // Use the Microsoft official fr-CA regional setting date format, for consistency between presentation and SharePoint backend format
        L: "DD/MM/YYYY"
    }
});


// GSoft namespace root
window.GSoft = window.GSoft || {};

// GSoft.Dynamite namespace root
window.GSoft.Dynamite = window.GSoft.Dynamite || {};


// GSoft.Dynamite Client namespace root
window.GSoft.Dynamite.Client = window.GSoft.Dynamite.Client || {};


// ====================
// Core module
// ====================
(function (Core, $, undefined) {

    Core.initialize = function (params) {

    };
} (GSoft.Dynamite.Client.Core = GSoft.Dynamite.Client.Core || {}, jq110));

// ====================
// Resources module
// ====================
(function(Res, $, undefined) {
    // SharePoint resource files can be fetched through the OOTB ScriptResource.ashx HttpHandler.
    // However, all resource files that get loaded tend to hog the Res namespace exclusivaly,
    // leading to problems when you wanna have multiple resources files loaded on the same page.
    //
    // The solution is that, before applying any KO bindings in your page, call this ensure method
    // which will sequentially load all the pre-req resourfiles (or at least confirm than they were 
    // already loaded) then execture your binding-applying snippet of code.
    //
    // For example, IntactNet.Res.ensureResThenExecute(["IFC.IntactNet", "IFC.IntactNet.News"], function () { // apply some bindings here });
    Res.ensureResThenExecute = function(prerequisiteResourceFiles, funcToExecute) {
        innerEnsureResThenExecute(prerequisiteResourceFiles, prerequisiteResourceFiles, funcToExecute);
    };

    function innerEnsureResThenExecute(allPrerequisiteResourceFiles, restOfPrerequisiteResourceFiles, funcToExecute) {
        if (!$.isArray(allPrerequisiteResourceFiles) || allPrerequisiteResourceFiles.length == 0
            || !$.isArray(restOfPrerequisiteResourceFiles) || restOfPrerequisiteResourceFiles.length == 0) {
            throw new Exception("Only call ensureResThenExecute with a non-empty array. E.g. ['IFC.IntactNet', 'IFC.IntactNet.News']");
        }

        if (restOfPrerequisiteResourceFiles.length > 0) {
            // many resource file dependencies: fetch the first then do the tail recursively
            var first = restOfPrerequisiteResourceFiles[0];
            var tail = _.tail(restOfPrerequisiteResourceFiles);

            if (!Res[first]) {
                // this file in particular was never fetched yet, wait for it
                Res[first] = "fetching"; // flag this as "currently fetching" so that we don't attempt to fetch it multiple times (and wait for it to 

                $.getScript(formatScriptResxLink(first))
                    .done(function() {
                        // when we load the SharePoint Resx script from its dedicated handler,
                        // it automatically assigns itself to the window.Res global object,
                        // wiping out any previously assigned value (which is why we're maintaining
                        // references to those other resource data in our own Res object).
                        Res[first] = window.Res;
                    })
                    .fail(function() { console.log("Failed to load resource file for module: " + first); });
            }

            if (tail.length > 0) {
                innerEnsureResThenExecute(allPrerequisiteResourceFiles, tail, funcToExecute);
            } else {
                waitUntilAllResourceFilesAreLoadedThenExecute(allPrerequisiteResourceFiles, funcToExecute);
            }
        }
    }

    function waitUntilAllResourceFilesAreLoadedThenExecute(allPrerequisiteResourceFiles, funcToExecute) {
            setTimeout(function() {
                var allLoaded = _.all(allPrerequisiteResourceFiles, function(prereqFileKey) {
                    return Res[prereqFileKey] && Res[prereqFileKey] != "fetching";
                });

                if (allLoaded) {
                    // We're ready, all resource file AJAX calls have come back
                    funcToExecute();
                } else {
                    // still waiting for some resource file to load, try again a bit later
                    waitUntilAllResourceFilesAreLoadedThenExecute(allPrerequisiteResourceFiles, funcToExecute);
                }
            }, 100);
        }

        function formatScriptResxLink(resourceFileName) {
            return GSoft.Dynamite.Client.Utils.CurrentWebUrl + "/_layouts/scriptresx.ashx?culture=" + currentCulture() + "&name=" + resourceFileName;
        }

        function currentCulture() {
            if (_spPageContextInfo.currentLanguage == 1033) {
                return "en-US";
            } else {
                return "fr-FR";
            }
        }

        return Res;
    }

    (GSoft.Dynamite.Client.Res = GSoft.Dynamite.Client.Res || {}, jq110));

    // ====================
    // Edit-mode Metadata Panel module
    // ====================
    (function(MetadataPanel, $, undefined) {
        MetadataPanel.viewModelInstance = null;

        MetadataPanel.initialize = function(tabs) {
            $(document).ready(function() {
                if (MetadataPanel.viewModelInstance == null) {
                    if ($("#metadata-panel").length == 1) {
                        MetadataPanel.viewModelInstance = new MetadataPanel.MetadataPanelViewModel(tabs);

                        // support only one metadata panel per page
                        ko.applyBindings(MetadataPanel.viewModelInstance, $("#metadata-panel")[0]);
                    }
                }
            });
        };

        MetadataPanel.MetadataPanelViewModel = function(tabs) {
            var self = this;

            self.tabs = ko.observableArray(tabs);

            self.findTab = function(tabId) {
                return _.find(self.tabs(), function(oneTab) {
                    return oneTab.id() == tabId;
                });
            };

            self.toggleTab = function(tab) {
                if (!tab.isSelected()) {
                    // un-select all the other tabs then activate the newly selected one
                    _.each(self.tabs(), function(oneOfTheTabs) {
                        oneOfTheTabs.isSelected(false);
                    });

                    tab.isSelected(true);
                } else {
                    // tab was already selected, just hid it (leaving just a row of unselected tabs)
                    tab.isSelected(false);
                }
            };

            self.lastSelectedTab = null;

            self.toggleAllTabs = function() {
                var alreadySelected = _.find(self.tabs(), function(oneTab) {
                    return oneTab.isSelected();
                });

                if (alreadySelected) {
                    // store the selected one until we toggleAll again
                    self.lastSelectedTab = alreadySelected;
                    alreadySelected.isSelected(false);

                    // deselect all
                    _.each(self.tabs(), function(oneOfTheTabs) {
                        oneOfTheTabs.isSelected(false);
                    });
                } else if (self.lastSelectedTab) {
                    // re-select the one that previously was
                    self.lastSelectedTab.isSelected(true);
                } else {
                    // just open the first tab by default
                    self.tabs()[0].isSelected(true);
                }
            };
        };

        MetadataPanel.Tab = function(id, res, isSelected) {
            var self = this;

            self.id = ko.observable(id);
            self.resourceString = ko.observable(res);
            self.isSelected = ko.observable(isSelected);
        };

        return MetadataPanel;
    }(GSoft.Dynamite.Client.MetadataPanel = GSoft.Dynamite.Client.MetadataPanel || {}, jq110));

    // ====================
    // Utils module
    // ====================
    (function(Utils, $, undefined) {
        Utils.CurrentWebUrl = null;
        Utils.ParentFolderUrl = "#";

        Utils.initialize = function(params) {

        };

        Utils.shortenAndEllipsis = function(text, size) {
            if (text != null && text.length > size) {
                text = text.substring(0, size);

                // find the last blank space to cut the text
                text = text.substring(0, text.lastIndexOf(' ')) + '...';
            }

            return text;
        };

        Utils.QueryObject = function() {
            var result = {}, queryString = location.search.slice(1),
                re = /([^&=]+)=([^&]*)/g, m;

            while (m = re.exec(queryString)) {
                result[decodeURIComponent(m[1])] = decodeURIComponent(m[2]);
            }

            return result;
        };

        Utils.initializeParentFolderLink = function() {
            if (Utils.ParentFolderUrl.length > 1) {
                ExecuteOrDelayUntilScriptLoaded(addLinkToSiteActions, "sp.js");
            }
        };

        function addLinkToSiteActions() {
            GSoft.Dynamite.Client.Res.ensureResThenExecute(["GSoft.Dynamite.Client"], function() {
                var newLink = $('<div class="parent-folder-link"><a title="'
                    + GSoft.Dynamite.Client.Res["GSoft.Dynamite.Client"].siteAction_OpenParentFolder
                    + '" href="' + Utils.ParentFolderUrl
                    + '"><img /></a></div>');
                var img = newLink.find("img");
                img.attr("src", "/_layouts/GSoft.Dynamite.Client/Img/icon_open_parent.png");
                $(".ms-siteactionscontainer .s4-breadcrumb-anchor").after(newLink);
            });
        };
    }(GSoft.Dynamite.Client.Utils = GSoft.Dynamite.Client.Utils || {}, jq110));


