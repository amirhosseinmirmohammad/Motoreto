
function scroll_to_class(chosen_class) {
    var nav_height = $('nav').outerHeight();
    var scroll_to = $(chosen_class).offset().top - nav_height;

    if ($(window).scrollTop() != scroll_to) {
        $('html, body').stop().animate({ scrollTop: scroll_to }, 1000);
    }
}


jQuery(document).ready(function () {
    var isStock = false;
    var radiogroups = [];
    /*
	    Fullscreen background
	*/

    //$.backstretch("/images/hero2-1440.png");
    /*
	    Multi Step Form
	*/

    function readURL(input) {
        if (input.files && input.files[0]) {
            $(input).next().addClass("ld")
            $('.' + $(input).parent().parent().parent().attr('class') + ' input[type="radio"]').next().addClass("trans").removeClass("bd")
            //$(input).parent().parent().parent($('input[type="radio"]').next().addClass("trans").removeClass("bd"))
            $('.' + $(input).parent().parent().parent().attr('class') + ' input[type="radio"]').prop("checked", false)

            //var reader = new FileReader();

            //reader.onload = function (e) {
            //    $('#profile-img-tag').attr('src', e.target.result);
            //}
            //reader.readAsDataURL(input.files[0]);
        }
    }

    function disable(input) {
        $(input).parent().parent().parent($('input[type="radio"]').next().removeClass("trans").removeClass("bd"))
        $(input).parent().parent().parent($('input[type="file"]').next().removeClass("ld"))
        $(input).next().addClass("bd")
        $(input).parent().parent().parent($('input[type="file"]').val(""))
    }

    $('.fieldset2 button.btn-previous').on('click', function () {
        getLastCategoryId()
    });

    $('.msf-form form fieldset:first-child').fadeIn('slow');

    // next step
    $('.msf-form form .btn-next').on('click', function () {
        var element = $(this);
        var parent = element.parent();
        var isSelected = false;

        var className = parent.attr('class').split(/\s+/)[0];
        //Validation
        $('.' + className + ' input[type="radio"]').each(function () {
            if ($(this).is(':checked') == true) {
                isSelected = true;
                isStock = false;
            }


            //CheckBoxes
            //if ($(this).is(':checkbox')) {
            //    if ($(this).is(':checked') == false) {
            //        var obg = this;
            //        var data = obg.dataset.validation;
            //        if (data != "" && data != undefined) {
            //            sweetAlert('توجه بفرمایید !', data, 'error');
            //            isStock = true;
            //            return false;
            //        }
            //    }
            //    else {
            //        isStock = false;
            //    }
            //}
            //CheckBoxes

            //Radios
            //if ($(this).is(':radio')) {
            //    var data = this.dataset.validation;
            //    if (data != "" && data != undefined) {
            //        if (radiogroups.indexOf($(this).attr('name')) == -1) {
            //            radiogroups.push($(this).attr('name'));
            //            var isSelected = false;
            //            $("input[name='" + $(this).attr('name') + "']").each(function () {
            //                if ($(this).is(':checked') == true) {
            //                    isSelected = true;
            //                    isStock = false;
            //                }
            //            });
            //            if (!isSelected) {
            //                if ($(this).is(':file')) {
            //                    var ext = $(this).val().split('.').pop().toLowerCase();
            //                    isStock = false;
            //                } else {
            //                    isStock = true;
            //                    radiogroups = []
            //                    sweetAlert('توجه بفرمایید !', " لطفا یکی از طرح ها را انتخاب کرده و یا طرح اختصاصی خود را آپلود کنید . ", 'error');
            //                }
            //            }
            //        }
            //    }
            //}

            ////Radios
            //if ($(this).is(':file')) {
            //    var ext = $(this).val().split('.').pop().toLowerCase();
            //    if ($.inArray(ext, ['png', 'jpg']) == -1 && $(this).attr('data-validation') != "" && $(this).attr('data-validation') != undefined) {
            //        var label = $("label[for='" + $(this).attr('id') + "']");
            //        var alert = 'فرمت ' + label.text() + ' معتبر نمیباشد . ';
            //        sweetAlert('توجه بفرمایید !', alert, 'error');
            //        isStock = true;
            //        return false;
            //    }
            //    else {
            //        isStock = false;
            //    }
            //}
            ////Radios

            //Inputes
            //var texval = $(this).val();
            //if ($(this).attr('data-validation') != "" && $(this).attr('data-validation') != undefined && texval == "") {
            //    sweetAlert('توجه بفرمایید !', $(this).attr('data-validation'), 'error');
            //    isStock = true;
            //    return false;
            //}
            //else {
            //    isStock = false;
            //}
        })
        if (!isSelected) {
            if ($('.' + className + ' input[type="file"]').length > 0) {
                if ($('.' + className + ' input[type="file"]').val() != "") {
                    isStock = false;
                } else {
                    isStock = true;
                    radiogroups = []
                    sweetAlert('توجه بفرمایید !', " لطفا یکی از طرح ها را انتخاب کرده و یا طرح اختصاصی خود را آپلود کنید . ", 'error');
                }
            } else {
                isStock = true;
                radiogroups = []
                sweetAlert('توجه بفرمایید !', " لطفا پکیج مورد نظر خود را انتخاب کنید . ", 'error');
            }

        }
        //Validation

        if (isStock == false) {
            $(this).parents('fieldset').fadeOut(400, function () {
                $(this).next().fadeIn();
                $(this).next($('input[type="file"]').change(function () {
                    readURL(this);
                }));
                $(this).next($('input[type="radio"]').change(function () {
                    disable(this);
                }));
                scroll_to_class('.msf-form');
            });
        }
    });

    // previous step
    $('.msf-form form .btn-previous').on('click', function () {
        $(this).parents('fieldset').fadeOut(400, function () {
            $(this).prev().fadeIn();
            scroll_to_class('.msf-form');
        });
    });

    jQuery('.category-item').on('click', function () {
        var element = jQuery(this);
        if (element.is(':radio')) {
            element.before().prop("checked", true);
        }
    });
});

function changePage(next, categoryId, element) {
    if (next) {
        //window.location = "/Place/Apply/" + categoryId;
    } else {
        element.parents('fieldset').fadeOut(400, function () {
            $(this).next().fadeIn();
            if ($(this).next().hasClass('fieldset2')) {
                getValueAtIndex(3)
            }
            scroll_to_class('.msf-form');
        });
    }
}

function getValueAtIndex(index) {
    var str = window.location.href;
    return str.split("/")[index];
}

function getLastCategoryId() {
    $.ajax({
        url: "/Home/GetParentCategoryId",
        contentType: "application/json; charset=utf-8",
        data: { CategoryId: getValueAtIndex(5) },
        dataType: "json",
        type: "GET",
        success: function (data) {
            //if (data["error"] != "true" && data["data"] != null) {
            //    window.location = "/Home/Index/" + data["data"];
            //} else {
            //    window.location = "/";
            //}

        },
        error: function (jqhxr, statusText, error) {
            sweetAlert('توجه بفرمایید !', " خطایی رخ داده است لطفا مجدد تلاش فرمایید . ", 'error');
        }
    });
}

function checkForsubs(categoryId, element) {
    $.ajax({
        url: "/Home/CheckSubCategories",
        contentType: "application/json; charset=utf-8",
        data: { CategoryId: categoryId },
        dataType: "json",
        type: "GET",
        success: function (data) {
            if (data["error"] != "true") {
                changePage(true, categoryId, element)
            }

        },
        error: function (jqhxr, statusText, error) {
            sweetAlert('توجه بفرمایید !', " خطایی رخ داده است لطفا مجدد تلاش فرمایید . ", 'error');
        }
    });
}


$("form").submit(function (event) {
    event.preventDefault();
    if (1) {
        swal({
            title: "آیا میخواهید سفارش خود را نهایی و ثبت کنید ؟",
            text: "پس از ثبت اطلاعات میبایست منتظر تایید مدیریت و در نهایت هماهنگی لازم جهت ارسال کالا بمانید .",
            type: "warning",
            showCancelButton: true,
            confirmButtonClass: "btn-danger",
            cancelButtonText: "خیر قصد بررسی مجدد موارد را دارم",
            confirmButtonText: "بله لطفا سفارش من را ثبت کنید",
            closeOnConfirm: false
        },
function (isConfirm) {
    if (isConfirm) {
        document.getElementById("ProForm").submit();
    }
});
    }
    else {
        document.getElementById("ProForm").submit();
    }
});

function setRadio(image) {

}

function switchToPage4() {
    $('.fieldset1').css('display', 'none')
    $('.fieldset4').css('display', 'block')
}

