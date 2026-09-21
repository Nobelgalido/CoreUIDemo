angular.module("useraccount", ["app"])

    .controller("accountsController", function ($scope, $location, $http, growl) {
        var vm = this;

        vm.RoleList = ["user", "manager", "admin"];

        vm.ChangePassword = {};

        var namePattern = /^[a-zA-Z ]+$/;

        $scope.Init = function () {
            $http({
                method: "POST",
                url: "/Settings/GetAccounts",
                arguments: { "Content-Type": "application/json" }
            }).then(function (data) {
                vm.AccountList = data.data.accountList;
            });

        };

        $scope.NewAccount = function () {
            vm.ModalHeader = "New";

            vm.Modal = { Role: "user" };

            ShowModal("AccountModal");
        };

        $scope.EditAccount = function (value) {
            vm.ModalHeader = "Edit";

            vm.Modal = angular.copy(value);

            ShowModal("AccountModal");
        };

        $scope.Save = function () {

            if (vm.Modal.Username == "" || vm.Modal.Username == null) {
                growl.error("Please input Username");
            }
            else if (vm.ModalHeader === "New" && (vm.Modal.Password == "" || vm.Modal.Password == null)) {
                growl.error("Please input Password");
            }
            else if (vm.ModalHeader === "New" && vm.Modal.Password.length < 6) {
                growl.error("Password must be at least 6 characters");
            }
            else if (vm.Modal.FirstName == "" || vm.Modal.FirstName == null) {
                growl.error("Please input First Name");
            }
            else if (!namePattern.test(vm.Modal.FirstName)) {
                growl.error("First Name must contain letters only");
            }
            else if (vm.Modal.LastName == "" || vm.Modal.LastName == null) {
                growl.error("Please input Last Name");
            }
            else if (!namePattern.test(vm.Modal.LastName)) {
                growl.error("Last Name must contain letters only");
            }
            else if (vm.Modal.Role == "" || vm.Modal.Role == null) {
                growl.error("Please select Role");
            }
            else {
                $http({
                    method: "POST",
                    url: "/Settings/SaveNewAccount",
                    data: {
                        account: vm.Modal,
                        role: vm.Modal.Role
                    }
                }).then(function (data) {
                    PopUpMessage(data.data);

                    $scope.Init();

                    if (data.data.message == "Saved") {
                        HideModal("AccountModal");
                    }
                });
            }
        };

        $("#firstName").keypress(function (event) {
            var inputValue = event.which;

            if (!(inputValue >= 65 && inputValue <= 90) && !(inputValue >= 97 && inputValue <= 122) && inputValue != 32) {
                event.preventDefault();
            }
        });


        $('#lastName').keypress(function (event) {
            var inputValue = event.which;

            if (!(inputValue >= 65 && inputValue <= 90) && !(inputValue >= 97 && inputValue <= 122) && inputValue != 32) {
                event.preventDefault();
            }
        });

        $scope.UpdatePassword = function (value) {

            vm.Change = angular.copy(value);

            ShowModal("ChangePasswordModal");
        };

        $scope.ChangePassword = function () {

            if (vm.Change.NewPassword == "" || vm.Change.NewPassword == null) {
                growl.error("Please input New Password");
            }
            else if (vm.Change.ConfirmPassword == "" || vm.Change.ConfirmPassword == null) {
                growl.error("Please input Confirm Password");
            }
            else if (vm.Change.NewPassword.length < 6) {
                growl.error("Password must be at least 6 characters");
            }
            else {
                if (vm.Change.NewPassword != vm.Change.ConfirmPassword) {
                    growl.error("Password Not Match!");
                }
                else {
                    $http({
                        method: "POST",
                        url: "/Settings/AdminChangePassword",
                        data: {
                            account: vm.Change.ID,
                            password: vm.Change.NewPassword
                        }

                    }).then(function (data) {
                        if (data.data.errorMessage == "") {
                            growl.success("Password Successfully Changed");

                            $scope.Init();

                            HideModal("ChangePasswordModal");
                        }
                        else {
                            growl.error(data.data.errorMessage)

                            vm.Change.NewPassword = "";

                            vm.Change.ConfirmPassword = "";
                        }
                    });
                }
            }
        };

        $scope.UpdateStatus = function (value) {

            vm.Status = angular.copy(value);

            ShowModal("UpdateStatusModal");
        };

        $scope.SaveStatus = function () {
            if (vm.Status.ConfirmPassword == "" || vm.Status.ConfirmPassword == null) {
                growl.error("Please input Password to proceed");
            }
            else {
                $http({
                    method: "POST",
                    url: "/Settings/UpdateStatus",
                    data: {
                        account: vm.Status.ID,
                        password: vm.Status.ConfirmPassword
                    }
                }).then(function (data) {
                    if (data.data.errorMessage == "") {
                        growl.success("Account Status Successfully Changed");

                        $scope.Init();

                        HideModal("UpdateStatusModal");
                    }
                    else {
                        growl.error(data.data.errorMessage)

                        vm.Status.ConfirmPassword = "";

                    }

                });
            }

        }
    });
