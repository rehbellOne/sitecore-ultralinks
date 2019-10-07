define(["jquery", "sitecore"], function ($, Sitecore) {
    "use strict";
   
    var model = Sitecore.Definitions.Models.ComponentModel.extend(
      {
        initialize: function (attributes) {
          this._super();
          this.set("json", null);
        },
		add: function (data) {

			var json = this.get("json");
			if (json === null)
				json = new Array();

			// this is done because array.push changes the array to an object which then do no work on the SPEAK listcontrol.
			var newArray = new Array(json.length + 1);
			for (var i = json.length - 1; i >= 0; i--)
				newArray[i + 1] = json[i];
			newArray[0] = data;

			this.set("json", newArray);
		
		},
		insert: function(data, index) {
			var json = this.get("json");
			if (json === null)
				json = new Array();
			// this is done because array.push changes the array to an object which then do no work on the SPEAK listcontrol.
			var newArray = new Array(json.length + 1);

			if (index === null || index === undefined) {

				for (var i = 0; i < json.length + 1; i++) {
					newArray[i] = json[i];
				}
				newArray[json.length] = data;
			}
			else {
				var j = 0;

				for (var i = 0; i < json.length + 1; i++) {

					if (i != index) {
						newArray[i] = json[j];

						j += 1;

					} else {
						newArray[i] = data;
					}
				}
			}

			this.set("json", newArray);
		},
        remove: function (data) {
	        var json = this.get("json");
	        if (json === null)
				return;

			var found = json[data];

			if (found !== undefined && found !== null) {

				var newArray = new Array(json.length - 1);

				var j = 0;

				for (var i = 0; i < json.length; i++) {
					if (i != data) {
						newArray[j] = json[i];
						j += 1;
					}
				}

				this.set("json", newArray);
			}
		}

      }
    );
   
    var view = Sitecore.Definitions.Views.ComponentView.extend(
      {
        initialize: function () {
          this._super();
          this.model.set("json", null);
        }
      }
    );
   
    Sitecore.Factories.createComponent("UltraLinksDataSource", model, view, "script[type='text/x-sitecore-ultralinksdatasource']");
  });