/// <reference path="jquery-1.4.1-vsdoc.js" />
/// <reference path="jquery-ui-1.8.16.min.js" />

var site = { baseUrl: "/" };

(function ($) {
    $.fn.userLookup = function () {
        $(this).each(function () {
            var id = 0;

            $(this).autocomplete({
                source: function (request, response) {
                    var myId = ++id;
                    $.ajax({
                        url: site.baseUrl + "Account/SearchUsers",
                        dataType: "json",
                        data: { q: request.term },
                        success: function (data) {
                            if (myId == id) {
                                response($.map(data, function (item) {
                                    return { label: item.username, value: item.username };
                                }));
                            }
                        }
                    });
                },
                minLength: 1
            });

            $(this).focusout(function () {
                ++id;
                $(this).removeClass("ui-autocomplete-loading");
            });
        });
    };
})(jQuery);

(function ($) {
    $(function () {
        $(".UserLookup").userLookup();
    });
})(jQuery);
