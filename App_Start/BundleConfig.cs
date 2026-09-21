using System.Web;
using System.Web.Optimization;

namespace CoreUIDemo
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/build/css/style.css",
                      "~/Content/vendor/@coreui/icons/css/free.min.css",
                      "~/Content/vendor/simplebar/css/simplebar.css",
                      "~/Content/vendor/growl/angular-growl.min.css",
                      "~/Content/Site.css"
                      ));

            // coreui.bundle.min.js is built by a modern toolchain whose output the ~2013-era
            // Microsoft.Ajax.Utilities minifier (System.Web.Optimization's default JS transform)
            // cannot parse — it throws a NullReferenceException instead of falling back, the way
            // the CSS minifier does for unparseable CSS. Hitting the bundle URL runs the minify
            // transform regardless of BundleTable.EnableOptimizations (that flag only controls
            // whether Scripts.Render links the bundle vs. individual files) — so Transforms.Clear()
            // is required here, not just #if DEBUG below. Every file in this bundle is already
            // minified/production-ready, so skipping the transform costs nothing.
            var scriptsBundle = new ScriptBundle("~/bundles/scripts").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Content/vendor/@coreui/coreui/js/coreui.bundle.min.js",
                "~/Content/vendor/simplebar/js/simplebar.min.js",
                "~/Scripts/angular.min.js",
                "~/Scripts/angular-growl.min.js"
                );
            scriptsBundle.Transforms.Clear();
            bundles.Add(scriptsBundle);

            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                "~/App/GrowlConfig.js",
                "~/App/App.js",
                "~/App/Controller/Login.js",
                "~/App/Controller/UserAccounts.js"
                ));

#if DEBUG
            BundleTable.EnableOptimizations = false;
#else
            BundleTable.EnableOptimizations = true;
#endif
        }
    }
}
