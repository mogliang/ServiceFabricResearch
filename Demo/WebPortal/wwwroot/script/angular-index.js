(function (angular) {
	var app = angular.module("app", [])
		.controller("home", ['$scope', '$window', '$http', '$interval', function ($scope, $window, $http, $interval) {
			$scope.title = 'Fabric Service Demo.';
			$scope.newagent = {};
			$scope.agents = [];
			$scope.waiting = false;

			$scope.registeragent = function () {
				$scope.waiting = true;
				$http.put('/api/Agent/' + $scope.newagent.compid,
					{
						'EmployeeID': $scope.newagent.agentid,
						'Name': $scope.newagent.name,
						'Title': 'Support Engineer'
					}).then(
						function (result) {
							$scope.waiting = false;
						},
						function (error) {
							$scope.waiting = false;
						}
					);
			};

			var pollwaiting = false;
			$interval(function () {

				if (pollwaiting)
					return;
				else
					pollwaiting = true;

				$http.get('/api/Agent/' + $scope.newagent.compid).then(
					function (result) {
						$scope.agents = result.data;
						pollwaiting = false;
					},
					function (error) {
						pollwaiting = false;
					});
			}, 1000);
		}]);
})(angular)