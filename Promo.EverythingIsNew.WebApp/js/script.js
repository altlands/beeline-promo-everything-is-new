$(function () {
    $(document).on('click', ".checkbox", function (e) {
        if ($(e.target).closest("a").length) return true;

        var $hidden = $(this).find("input");
        var $cont = $(this);//.closest(".checkbox");

        if ($hidden.is(":checked")) {
            $cont.removeClass("checked");
            $hidden.prop('checked', false);
        } else {
            $cont.addClass("checked");
            $hidden.prop('checked', true);
        }

        return false;
    });

    $(document).on('click', ".fold-link", function (e) {
        var $link = $(this),
            $foldable = $link.next(".foldable");

        $foldable.slideToggle(300);
        $link.toggleClass("open");

        return false;
    });

    $(document).on('click', ".success-block", function (e) {
        var T = $(this),
            $circle = T.find(".circle");

        setTimeout(function () {
            $circle.addClass("show");
        }, 500);

        return false;
    });

    $("#submit-index").click(function () {
        $('#form-index').submit();
    });

    /*  popup  */
    $(document).on("click", ".popup-cont .popup-close, .popup-cont .popup-mask", function () {
        $(this).closest(".popup-cont").hide();
        return false;
    });

    function showPopup(obj) {
        var $obj = $(obj);

        if ($obj.length != 1) return;

        $obj.show().siblings(".popup").hide();
        $obj.closest(".popup-cont").show();

        var top = $(document).scrollTop() + ($(window).height() - $obj.outerHeight()) * 0.4;
        $obj.css("top", parseInt(top) + "px");
        return false;
    }

    // usage
    $(document).on("click", ".wrong-email", function () {
        showPopup(".wrong-email-popup");
    });
        /* /popup  */
});