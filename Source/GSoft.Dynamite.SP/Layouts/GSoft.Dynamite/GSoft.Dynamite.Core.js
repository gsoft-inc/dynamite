// Core Javascript file for GSoft.Dynamite

// Chrome-JS intermitted crash fix (otherwise the ribbon breaks in Chrome 9 times out of 10)
// NOTE: Current chrome fix is buggy.  It adds a second link to "my profile" and breaks the Web Part zones.
/*
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
}*/

moment.locale('fr', {
    longDateFormat: {
        // Use the Microsoft official fr-CA regional setting date format, for consistency between presentation and SharePoint backend format
        L: "YYYY-MM-DD"
    }
});


moment.locale('en', {
    longDateFormat: {
        // Use the Microsoft official en-US regional setting date format, for consistency between presentation and SharePoint backend format
        L: "DD/MM/YYYY"
    }
});

// GSoft namespace root
window.GSoft = window.GSoft || {};

// GSoft.Dynamite namespace root
window.GSoft.Dynamite = window.GSoft.Dynamite || {};

// ====================
// Core module
// ====================
(function (Core, $, undefined) {

    Core.initialize = function (params) {

    };
}(GSoft.Dynamite.Core = GSoft.Dynamite.Core || {}, jq110));

// ====================
// Resources module
// ====================
(function (Resource, $, undefined) {
    // SharePoint resource files can be fetched through the OOTB ScriptResource.ashx HttpHandler.
    // However, all resource files that get loaded tend to hog the Resource namespace exclusivaly,
    // leading to problems when you wanna have multiple resources files loaded on the same page.
    //
    // The solution is that, before applying any KO bindings in your page, call this ensure method
    // which will sequentially load all the pre-req resourfiles (or at least confirm than they were 
    // already loaded) then execture your binding-applying snippet of code.
    //
    // For example, GSoft.Dynamite.Resource.ensureResourceThenExecute(["GSoft.Dynamite", "GSoft.Dynamite.News"], function () { // apply some bindings here });
    Resource.ensureResourceThenExecute = function (prerequisiteResourceFiles, functionToExecute) {
        innerensureResourceThenExecute(prerequisiteResourceFiles, prerequisiteResourceFiles, functionToExecute);
    };

    function innerensureResourceThenExecute(allPrerequisiteResourceFiles, restOfPrerequisiteResourceFiles, functionToExecute) {
        if (!$.isArray(allPrerequisiteResourceFiles) ||
            allPrerequisiteResourceFiles.length == 0 ||
            !$.isArray(restOfPrerequisiteResourceFiles) ||
            restOfPrerequisiteResourceFiles.length == 0) {
            throw new Exception("Only call ensureResourceThenExecute with a non-empty array. E.g. ['GSoft.Dynamite', 'GSoft.Dynamite.News']");
        }

        if (restOfPrerequisiteResourceFiles.length > 0) {
            // many resource file dependencies: fetch the first then do the tail recursively
            var first = restOfPrerequisiteResourceFiles[0];
            var tail = _.tail(restOfPrerequisiteResourceFiles);

            if (!Resource[first]) {
                // this file in particular was never fetched yet, wait for it
                Resource[first] = "fetching"; // flag this as "currently fetching" so that we don't attempt to fetch it multiple times (and wait for it to 

                $.getScript(formatScriptResxLink(first))
                    .done(function () {
                        // when we load the SharePoint Resx script from its dedicated handler,
                        // it automatically assigns itself to the window.Resource global object,
                        // wiping out any previously assigned value (which is why we're maintaining
                        // references to those other resource data in our own Resource object).
                        Resource[first] = window.Res;
                    })
                    .fail(function () { console.log("Failed to load resource file for module: " + first); });
            }

            if (tail.length > 0) {
                innerensureResourceThenExecute(allPrerequisiteResourceFiles, tail, functionToExecute);
            } else {
                waitUntilAllResourceFilesAreLoadedThenExecute(allPrerequisiteResourceFiles, functionToExecute);
            }
        }
    }

    function waitUntilAllResourceFilesAreLoadedThenExecute(allPrerequisiteResourceFiles, functionToExecute) {
        setTimeout(function () {
            var allLoaded = _.all(allPrerequisiteResourceFiles, function (prereqFileKey) {
                return Resource[prereqFileKey] && Resource[prereqFileKey] != "fetching";
            });

            if (allLoaded) {
                // We're ready, all resource file AJAX calls have come back
                functionToExecute();
            } else {
                // still waiting for some resource file to load, try again a bit later
                waitUntilAllResourceFilesAreLoadedThenExecute(allPrerequisiteResourceFiles, functionToExecute);
            }
        }, 100);
    }

    function formatScriptResxLink(resourceFileName) {
        return GSoft.Dynamite.Utilities.CurrentWebUrl + GSoft.Dynamite.Utilities.LayoutFolder() + "scriptresx.ashx?culture=" + currentCulture() + "&name=" + resourceFileName;
    }

    function currentCulture() {
        if (_spPageContextInfo && _spPageContextInfo.currentLanguage == 1033) {
            return "en-US";
        } else if (_spPageContextInfo && _spPageContextInfo.currentLanguage == 1036) {
            return "fr-FR";
        } else {
            return "en-US";
        }
    }

    return Resource;
}(GSoft.Dynamite.Resource = GSoft.Dynamite.Resource || {}, jq110));

// ====================
// File loader module
// ====================
(function (FileLoader, $, undefined) {

    // GET files and return deferred object when done.
    // See http://api.jquery.com/category/deferred-object/ for more information on deferred objects in jQuery.
    // Usage example: 
    // GSoft.Dynamite.FileLoader.load("/_layouts/folder/file1.html", "/_layouts/folder/file2.html").done(function(files) { /*Use files here*/});,
    FileLoader.load = function () {

        // Build promises and resolve them when GET operation is done
        var promises = [];
        for (var i = 0; i < (arguments.length) ; i++) {
            var file = arguments[i];
            promises.push($.get(file));
        }

        // If promises were created, return deferred object with promises
        if (promises.length > 0) {
            return $.when.apply($, promises);
        } else {

            // Return empty resolved deferred object
            return $.when();
        }
    };
}(GSoft.Dynamite.FileLoader = GSoft.Dynamite.FileLoader || {}, jq110));

// ====================
// Edit-mode Metadata Panel module
// ====================
(function (MetadataPanel, $, undefined) {
    MetadataPanel.viewModelInstance = null;

    MetadataPanel.initialize = function (tabs) {
        $(document).ready(function () {
            if (MetadataPanel.viewModelInstance == null) {
                if ($("#metadata-panel").length == 1) {
                    MetadataPanel.viewModelInstance = new MetadataPanel.MetadataPanelViewModel(tabs);

                    // support only one metadata panel per page
                    ko.applyBindings(MetadataPanel.viewModelInstance, $("#metadata-panel")[0]);
                }
            }
        });
    };

    MetadataPanel.MetadataPanelViewModel = function (tabs) {
        var self = this;

        self.tabs = ko.observableArray(tabs);

        self.findTab = function (tabId) {
            return _.find(self.tabs(), function (oneTab) {
                return oneTab.id() == tabId;
            });
        };

        self.toggleTab = function (tab) {
            if (!tab.isSelected()) {
                // un-select all the other tabs then activate the newly selected one
                _.each(self.tabs(), function (oneOfTheTabs) {
                    oneOfTheTabs.isSelected(false);
                });

                tab.isSelected(true);
            } else {
                // tab was already selected, just hid it (leaving just a row of unselected tabs)
                tab.isSelected(false);
            }
        };

        self.lastSelectedTab = null;

        self.toggleAllTabs = function () {
            var alreadySelected = _.find(self.tabs(), function (oneTab) {
                return oneTab.isSelected();
            });

            if (alreadySelected) {
                // store the selected one until we toggleAll again
                self.lastSelectedTab = alreadySelected;
                alreadySelected.isSelected(false);

                // deselect all
                _.each(self.tabs(), function (oneOfTheTabs) {
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

    MetadataPanel.Tab = function (id, res, isSelected) {
        var self = this;

        self.id = ko.observable(id);
        self.resourceString = ko.observable(res);
        self.isSelected = ko.observable(isSelected);
    };

    return MetadataPanel;
}(GSoft.Dynamite.MetadataPanel = GSoft.Dynamite.MetadataPanel || {}, jq110));

// ====================
// Utilities module
// ====================
(function (Utilities, $, undefined) {
    Utilities.CurrentWebUrl = null;
    Utilities.ParentFolderUrl = "#";

    Utilities.initialize = function (params) {

    };

    Utilities.ExtractTaxonomyInfo = function (taxonomyValue) {
        if (taxonomyValue) {

            var results = [];
            var regex = /L0\|#0([a-f0-9]{8}(?:-[a-f0-9]{4}){3}-[a-f0-9]{12})\|([\d \w \s áàâäãåçéèêëíìîïñóòôöõúùûüýÿæœÁÀÂÄÃÅÇÉÈÊËÍÌÎÏÑÓÒÔÖÕÚÙÛÜÝŸÆŒ']*);/gi;

            while ((match = regex.exec(taxonomyValue)) !== null) {
                results.push({ id: match[1], label: match[2] });
            }

            return results;
        }

        return { id: undefined, label: undefined };
    };

    Utilities.LayoutFolder = function () {
        if (_spPageContextInfo && _spPageContextInfo.webUIVersion === 15) {
            return "/_layouts/15/";
        }

        return "/_layouts/";
    }

    Utilities.shortenAndEllipsis = function (text, size) {
        if (text != null && text.length > size) {
            text = text.substring(0, size);

            // find the last blank space to cut the text
            text = text.substring(0, text.lastIndexOf(' ')) + '...';
        }

        return text;
    };

    Utilities.QueryObject = function () {
        var result = {},
            queryString = location.search.slice(1),
            regex = /([^&=]+)=([^&]*)/g,
            match;

        while (match = regex.exec(queryString)) {
            result[decodeURIComponent(match[1])] = decodeURIComponent(match[2]);
        }

        return result;
    };

    Utilities.initializeParentFolderLink = function () {
        if (Utilities.ParentFolderUrl.length > 1) {
            ExecuteOrDelayUntilScriptLoaded(addLinkToSiteActions, "sp.js");
        }
    };

    // When you are on a cross site publishing site, and you need to get the absolute picture file from a managed property of the type image field,
    // this method returns an absolute path of the image.
    // The method parses the value with a regex instead of jQuery because jq add an element to the DOM and so the browser gets the url with a 404.
    // elementString : the string version of the html element of the image (the value of the image field type)
    // spSiteUrl : The value of the managed property spSiteUrl
    Utilities.GetAbsoluteImagePath = function (elementString, spSiteUrl) {
        var sourceAttributeMatch = elementString.match(/src=\"(.+?)\"/i);
        var imageSource = sourceAttributeMatch.length > 1 ? sourceAttributeMatch[1] : null;

        // If the image URL is relative, prepend the site URL
        if (imageSource && imageSource.startsWith("/")) {

            return spSiteUrl + imageSource;
        }
        return "";
    };

    // Provides the mecanism to create a accordion and show/hide element when the click event is handled on the header's title.
    // clickableElement : represents the title of each section.
    // visibleCssClass : The class added when the section below the clickable element is shown.
    // Exemple : 'div.experience-steps> h3' => represents all the sections' titles
    // <div class='experience-steps'>
    // <h3></h3>
    // <div></div>
    // <h3></h3>
    // <div></div>
    // </div>
    Utilities.ToggleElement = function (clickableElement, visibleCssClass) {
        $(document).ready(function () {
            $(clickableElement).click(function () {
                var $nextSection = $(this).next();
                if ($nextSection.is(":hidden")) {
                    $(this).addClass(visibleCssClass);
                    $nextSection.show("slow");
                } else {
                    $(this).removeClass(visibleCssClass);
                    $nextSection.slideUp();
                }
            });
        });
    }

    function addLinkToSiteActions() {
        GSoft.Dynamite.Resource.ensureResourceThenExecute(["GSoft.Dynamite"], function () {
            var newLink = $('<div class="parent-folder-link"><a title="'
                + GSoft.Dynamite.Resource["GSoft.Dynamite"].siteAction_OpenParentFolder
                + '" href="' + Utilities.ParentFolderUrl
                + '"><img /></a></div>');
            var img = newLink.find("img");
            img.attr("src", Utilities.LayoutFolder() + "GSoft.Dynamite/Img/icon_open_parent.png");
            $(".ms-siteactionscontainer .s4-breadcrumb-anchor").after(newLink);
        });
    };
}(GSoft.Dynamite.Utilities = GSoft.Dynamite.Utilities || {}, jq110));

// ====================
// DisplayTemplateHelper module
// ====================
(function (DisplayTemplateHelper, $, undefined) {
    
    DisplayTemplateHelper.ensureAbsoluteImageUrl = function (pictureUrlItemValue, spSiteUrl) {

        // Get search item values
        var siteUrlItemValue = spSiteUrl;

        // Check if indexed values aren't null or empty
        if ((pictureUrlItemValue != null) &&
			(!siteUrlItemValue.isNull && !siteUrlItemValue.isEmpty)) {

            // Get the image source attribute
            var sourceAttributeMatch = pictureUrlItemValue.match(/src=\"(.+?)\"/i);
            var imageSource = sourceAttributeMatch.length > 1 ? sourceAttributeMatch[1] : null;

            // If the image URL is relative, prepend the site URL
            if (imageSource.startsWith("/")) {
                var siteUrl = siteUrlItemValue.value;

                // Update the src attribute value
                pictureUrlItemValue = pictureUrlItemValue.replace(sourceAttributeMatch[1], siteUrl + imageSource);
            }
        }

        return pictureUrlItemValue;
    };

}(GSoft.Dynamite.DisplayTemplateHelper = GSoft.Dynamite.DisplayTemplateHelper || {}, jq110));


