
// Knockout binding addons
(function ($) {
    // Knockout customizations
    ko.bindingHandlers.placeholder = {
        // Thanks http://stackoverflow.com/a/16281561 for the shim approach
        init: function (element, valueAccessor) {
            var placeholderValue = valueAccessor();
            ko.applyBindingsToNode(element, { attr: { placeholder: placeholderValue } });

            // gotta re-apply shim during screen resize
            g_workspaceResizedHandlers.push(function () {
                $.placeholder.shim();
            });
        },
        update: function () {
            $.placeholder.shim();
        }
    };

    ko.bindingHandlers.slideVisible = {
        update: function (element, valueAccessor, allBindings) {
            // First get the latest data that we're bound to
            var value = valueAccessor();

            // Next, whether or not the supplied model property is observable, get its current value
            var valueUnwrapped = ko.unwrap(value);

            // Grab some more data from another binding property
            var duration = allBindings.get('slideDuration') || 400; // 400ms is default duration unless otherwise specified

            // Now manipulate the DOM element
            if (valueUnwrapped === true) {
                $(element).slideDown(duration); // Make the element visible
            } else {
                $(element).slideUp(duration);   // Make the element invisible
            }
        }
    };

    ko.bindingHandlers.fadeVisible = {
        update: function (element, valueAccessor, allBindings) {
            // First get the latest data that we're bound to
            var value = valueAccessor();

            // Next, whether or not the supplied model property is observable, get its current value
            var valueUnwrapped = ko.unwrap(value);

            // Grab some more data from another binding property
            var duration = allBindings.get('fadeDuration') || 400; // 400ms is default duration unless otherwise specified

            // Now manipulate the DOM element
            if (valueUnwrapped === true) {
                $(element).fadeIn(duration); // Make the element visible
            } else {
                $(element).hide();   // Make the element invisible
            }
        }
    };

    ko.bindingHandlers.tab = {
        update: function (element, valueAccessor) {

            // This will be called once when the binding is first applied to an element,
            // and again whenever the associated observable changes value.
            // Update the DOM element based on the supplied values here.
            var tab = valueAccessor();

            if (tab.isSelected()) {
                $(element).show();
            } else {
                $(element).hide();
            }
        }
    };

    ko.bindingHandlers.hoverTargetId = {};
    ko.bindingHandlers.hoverVisible = {
        init: function (element, valueAccessor, allBindingsAccessor) {

            var $hoverTarget = $("#" + ko.utils.unwrapObservable(allBindingsAccessor().hoverTargetId));

            var css = allBindingsAccessor.get('hoverTargetCss');

            var ishovering = ko.observable(false);
            var showIt = true;
            var currentTimer = null;

            ishovering.subscribe(function () {

                if (currentTimer) {
                    clearTimeout(currentTimer);
                }

                currentTimer = setTimeout(function () {
                    if (ishovering() == showIt) {
                        $(element).toggle(ishovering());

                        if (ishovering()) {
                            $hoverTarget.addClass(css);
                        } else {
                            $hoverTarget.removeClass(css);
                        }
                    }
                }, 500);

            });

            var showMenu = function () {
                showIt = true;
                ishovering(true);
            };

            var hideMenu = function () {
                showIt = false;
                ishovering(false);
            };

            ko.utils.registerEventHandler($hoverTarget, "mouseover", showMenu);
            ko.utils.registerEventHandler($hoverTarget, "mouseout", hideMenu);
            ko.utils.registerEventHandler($(element), "mouseover", showMenu);
            ko.utils.registerEventHandler($(element), "mouseout", hideMenu);
        }
    };
}(jq111));
