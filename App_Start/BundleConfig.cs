using System.Web.Optimization;

namespace CoreUIDemo
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            

            // Standard MVC 5 template bundle — referenced by _Layout.cshtml.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            // jQuery core — kept ONLY for jquery.validate.unobtrusive on the Login page.
            // CoreUI itself ships zero jQuery dependency (verified: no <script src> anywhere
            // in coreui-free-bootstrap-admin-template-v5.5.0-dist references jQuery).
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // CoreUI's own bundle — Bootstrap 5 JS + Popper + every CoreUI component, one file.
            // color-modes.js is deliberately NOT here: it must run in <head>, before first
            // paint, or the page flashes light before switching to the saved dark theme.
            // _Layout loads it as a plain <script> tag instead (see below).
            bundles.Add(new ScriptBundle("~/bundles/coreui").Include(
                      "~/Content/vendor/coreui/js/coreui.bundle.min.js"));

            // AngularJS core + growl
            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                      "~/Scripts/angular.js",
                      "~/Scripts/growl/build/angular-growl.js"));

            // Your app's own Angular module + controllers
            bundles.Add(new ScriptBundle("~/bundles/app").Include(
                      "~/Scripts/App/app.js",
                      "~/Scripts/App/Controller/Shared/main.controller.js",
                      "~/Scripts/App/Controller/Users/users.list.controller.js"));

            // Styles — CoreUI's compiled theme + icon font + our overrides, in that order.
            // NOTE the virtual path: "~/bundles/css", NOT the MVC template's "~/Content/css".
            // This project has a real Content/css/ directory on disk (site-overrides.css lives
            // there), and a bundle whose virtual path collides with an existing physical
            // directory gets served by IIS's static/directory handler instead of the bundling
            // module — a 403 or 404 on the stylesheet, and a completely unstyled page with no
            // build error and nothing in the server log. Keep every bundle path under
            // ~/bundles/ and the collision can never happen.
            bundles.Add(new StyleBundle("~/bundles/css").Include(
                      "~/Content/*.css"));

            // Separate bundle for angular-growl's own CSS (same ~/bundles/ rule)
            bundles.Add(new StyleBundle("~/bundles/growl-css").Include(
                      "~/Scripts/growl/build/angular-growl.css"));

#if DEBUG
            BundleTable.EnableOptimizations = false; // unminified during development — easier to debug
#else
            BundleTable.EnableOptimizations = true;
#endif
        }
    }
}