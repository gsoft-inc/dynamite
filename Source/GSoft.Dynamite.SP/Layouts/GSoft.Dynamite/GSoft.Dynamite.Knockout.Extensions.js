// This extender allow you to add paging functionality to any observableArray
ko.extenders.paging = function (target) {
    var _currentPageNumber = ko.observable(1);

    target.currentPageNumber = ko.computed({
        read: _currentPageNumber,
        write: function (newValue) {
            if (newValue > target.pageCount()) {
                _currentPageNumber(target.pageCount());
            } else if (newValue <= 0) {
                _currentPageNumber(1);
            } else {
                _currentPageNumber(newValue);
            }

            target.valueHasMutated();
        }
    });

    target.currentPageData = ko.computed(function () {
        return target()[target.currentPageNumber() - 1];
    });

    target.pageCount = ko.computed(function () {
        return target().length || 1;
    });

    target.isFirstPage = ko.computed(function () {
        return target.currentPageNumber() == 1;
    });

    target.isLastPage = ko.computed(function () {
        return target.currentPageNumber() == target.pageCount();
    });

    target.moveFirst = function () {
        target.currentPageNumber(1);
    };
    target.movePrevious = function () {
        target.currentPageNumber(parseInt(target.currentPageNumber()) - 1);
    };
    target.moveNext = function () {
        target.currentPageNumber(parseInt(target.currentPageNumber()) + 1);
    };
    target.moveLast = function () {
        target.currentPageNumber(target.pageCount());
    };
};