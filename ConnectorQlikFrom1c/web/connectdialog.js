define( ['qvangular',
		'text!СonnectorQlikFrom1c.webroot/connectdialog.ng.html',
		'css!СonnectorQlikFrom1c.webroot/connectdialog.css'
], function ( qvangular, template) {
	return {
		template: template,
		controller: ['$scope', 'input', function ( $scope, input ) {
			function init() {
				$scope.isEdit = input.editMode;
				$scope.id = input.instanceId;
				$scope.titleText = $scope.isEdit ? "Change QV Event Log connection" : "Add QV Event Log connection";
				$scope.saveButtonText = $scope.isEdit ? "Save changes" : "Create";
				$scope.isOkEnabled = false;

				$scope.name_connect = "";
				$scope.provider = "СonnectorQlikFrom1c.exe"; // Connector filename

				/*поля для подключения к системе*/
				$scope.cb_isClient = true;
				$scope.cb_isDomenAuth = true;
				$scope.connectionString = createCustomConnectionString($scope.provider);
				
				/*define param*/
				const lo_now = new Date();
				$scope.name_connect = "connect1c_" + lo_now.getHours() + lo_now.getMinutes();
				$scope.server = "dev01";
				$scope.database = "bf_3_1_markova";
				$scope.table_qv = "tableQV_" + lo_now.getHours() + lo_now.getMinutes();
				$scope.query = "ВЫБРАТЬ Код, Наименование, КоррСчет, Город ИЗ Справочник.Банки";
				/*end define param*/

				input.serverside.sendJsonRequest( "getInfo" ).then( function ( info ) {
					$scope.info = info.qMessage; /*сообщение что коннектор подключился успешно, вернуть из C#*/
					$scope.isOkEnabled = true;
				} );

				if ( $scope.isEdit ) {
					input.serverside.getConnection( $scope.id ).then( function ( result ) {
						$scope.name_connect = result.qName;
					} );
				}
			}


			/* Event handlers */

			$scope.onOKClicked = function () {
				$scope.connectionString = createCustomConnectionString( $scope.provider );
				
				if ( $scope.isEdit ) {
					input.serverside.modifyConnection( $scope.id,
						$scope.name_connect,
						$scope.connectionString,
						$scope.provider).then( function ( result ) {
							if ( result ) {
								$scope.destroyComponent();
							}
						} );
				} else {
					input.serverside.createNewConnection( $scope.name_connect, $scope.connectionString);
					$scope.destroyComponent();
				}
			};

			$scope.onEscape = $scope.onCancelClicked = function () {
				$scope.destroyComponent();
			};

			
			/* Helper functions */
			function createCustomConnectionString ( filename ) {
				var connectionstring = "";
				
				if ($scope.cb_isClient == true) {
					connectionstring += "Srvr=" + $scope.server + ";Ref=" + $scope.database + ";";
				}
				else {
					connectionstring += "File=" + $scope.path + ";";
				}

				if ($scope.userdb != "") {
					connectionstring += "UserDB=" + $scope.userdb + ";PasswordDB=" + $scope.passdb + ";";
				}
				
				connectionstring += "QV_Table=" + $scope.table_qv + ";Query=" + $scope.query + ";";
				connectionstring += "name_connect=" + $scope.name_connect + ";";
				return "CUSTOM CONNECT TO " + "\"provider=" + filename + ";" + connectionstring + "\"";
			}

			init();
		}]
	};
} );