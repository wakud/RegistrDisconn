// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
//(function ($) {
//	$.fn.checkboxTable = function () {
//		target = this;

//		// Клик по checkbox в шапке таблицы.
//		$(target).on('click', 'thead :checkbox', function () {
//			var check = this;
//			$(this).parents('table').find('tbody :checkbox').each(function () {
//				if ($(check).is(':checked')) {
//					$(this).prop('checked', true);
//					$(this).parents('tr').addClass('selected');
//				} else {
//					$(this).prop('checked', false);
//					$(this).parents('tr').removeClass('selected');
//				}
//			});
//		});

//		// Клик по checkbox в строке таблицы.
//		$(target).on('click', 'tbody :checkbox', function () {
//			var parents = $(this).parents('table');
//			if ($(this).is(':checked')) {
//				$(this).parents('tr').addClass('selected');
//				$(parents).find('thead :checkbox').prop('checked', true);
//			} else {
//				$(this).parents('tr').removeClass('selected');
//				if ($(parents).find('tbody :checkbox:checked').length == 0) {
//					$(parents).find('thead :checkbox').prop('checked', false);
//				}
//			}
//		});

//		// Клик по строке таблицы
//		$(target).on('click', 'tbody tr', function (event) {
//			if (event.target.tagName == 'TH' || event.target.tagName == 'TD') {
//				$(this).find(':checkbox').trigger('click');
//			}
//		});
//	};
//})(jQuery);