/*jslint browser: true*/
/*global $, jQuery ,AOS*/


(function ($) {

    "use strict";

    var $window = $(window),
        $body = $('body'),
        $appyMenu = $('.appy-menu'),
        $homeSlider = $('.home-slider'),
        $testiSlider = $('.testimonial-slider'),
        $screenShotsSlider = $('.screenshots-slider'),
        $countUp = $('.count-num span'),
        $teamSlider = $('.team-slider'),
        $blogSlider = $('.recent-blog-slider');


    /*START PRELOADER JS & REFRESH AOS*/
    $window.on('load', function () {
        $('.preloader').delay(350).fadeOut('slow');
        AOS.refresh();
    });
    /*END PRELOADER JS & REFRESH AOS*/

    $(document).ready(function () {

        /*START AOS JS*/
        AOS.init({
            disable: 'mobile',
            once: true,
            duration: 600
        });
        /*END AOS JS*/

        /*START SCROLL SPY JS*/
        $body.scrollspy({
            target: '#main_menu'
        });
        /*END SCROLL SPY JS*/

        /*START MENU JS*/
        $('a.scroll-section').on('click', function (e) {
            var anchor = $(this);
            var ancAtt = $(anchor.attr('href'));
            $('html, body').stop().animate({
                scrollTop: ancAtt.offset().top
            }, 1000);
            e.preventDefault();
        });

        $window.scroll(function () {
            var currentLink = $(this);
            if ($(currentLink).scrollTop() > 0) {
                $appyMenu.addClass("sticky");
            } else {
                $appyMenu.removeClass("sticky");
            }
        });
        /*END MENU JS*/

        /*START HOME SLIDER JS*/
        if ($homeSlider.length > 0) {
            $homeSlider.owlCarousel({
                loop: true,
                margin: 10,
                items: 1,
                animateOut: 'fadeOut',
                responsiveClass: true,
                navText: ["<i class='fa fa-chevron-left'></i>", "<i class='fa fa-chevron-right'></i>"],
                responsive: {
                    768: {
                        nav: true,
                    }
                }
            });
        }
        /*END HOME SLIDER JS*/

        /*START TESTIMONIAL SLIDER JS*/
        if ($testiSlider.length > 0) {
            $testiSlider.owlCarousel({
                loop: true,
                margin: 10,
                items: 1,
                responsiveClass: true
            });
        }
        /*END TESTIMONIAL SLIDER JS*/

        /*START SCREENSHOTS SLIDER JS*/
        if ($screenShotsSlider.length > 0) {
            $screenShotsSlider.owlCarousel({
                loop: true,
                responsiveClass: true,
                nav: true,
                animateOut: 'slideOutLeft',
                animateIn: 'zoomIn',
                dots: false,
                autoplay: true,
                autoplayTimeout: 400000,
                smartSpeed: 500,
                navSpeed: 200,
                center: true,
                items: 1
            });
        }
        /*END SCREENSHOTS SLIDER JS*/

        /*START COUNTUP JS*/
        $countUp.counterUp({
            delay: 10,
            time: 1500
        });
        /*END COUNTUP JS*/

        /*START TEAM SLIDER JS*/
        if ($teamSlider.length > 0) {
            $teamSlider.owlCarousel({
                loop: false,
                margin: 30,
                dots: true,
                nav: false,
                responsiveClass: true,
                responsive: {
                    0: {
                        items: 1
                    },
                    480: {
                        items: 2
                    },
                    768: {
                        items: 3
                    },
                    1200: {
                        items: 4
                    }
                }
            });
        }
        /*END TEAM SLIDER JS*/

        /*RECENT BLOG SLIDER JS*/
        if ($blogSlider.length > 0) {
            $blogSlider.owlCarousel({
                loop: false,
                margin: 30,
                dots: true,
                nav: false,
                slideBy: 1,
                responsiveClass: true,
                responsive: {
                    0: {
                        items: 1
                    },
                    768: {
                        items: 2
                    },
                    1200: {
                        items: 3
                    }
                }
            });
        }
        /*END RECENT BLOG SLIDER JS*/


    });

    /*$(window).on('resize', function(event){
        var windowSize = $(window).width();
        if(windowSize < 768){
            $('.dropdown-submenu a').on("click", function(e){
                $(this).next('ul').toggle();
                e.stopPropagation();
                e.preventDefault();
            });
        }
    });*/

}(jQuery));
