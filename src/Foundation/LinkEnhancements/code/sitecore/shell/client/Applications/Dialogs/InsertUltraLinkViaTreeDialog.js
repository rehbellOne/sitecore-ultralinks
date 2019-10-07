define(["sitecore"], function (Sitecore) {

    var linkTypeOptions = {
        external: "{5C510D92-6FA4-48EF-AF79-67EE126F5B54}",
        internal: "{012728CF-9457-43C5-97E9-CF68AF236402}",
        email: "{10D7940B-2946-4C07-802F-A2AD5BE012FC}",
        phone: "{1728B7EA-402B-425D-A009-C8D090EB7799}"
    };
  
	var addDataSourceItem = function(dataSource, nameObj, valueObj) {

		var name = nameObj.get("text");
		var value = valueObj.get("text");

		if (name === null || name === "") return;

		var items = dataSource.get("json");

		if (items === null) {

			dataSource.add({
				"AttributeName": name,
				"AttributeValue": value,
				"ItemId": null
			});
		}
		else {
			var index = -1;

			for (var i = 0; i < items.length; i++) {

				var item = items[i];

				if (item.AttributeName === name) {
					item.AttributeValue = value;
					index = i;
					break;
				}
			}

			if (index < 0) {
				dataSource.add({
					"AttributeName": name,
					"AttributeValue": value,
					"ItemId": null
				});
			}
		}

	};

	var updateDataSourceItem = function(dataSource, nameObj, valueObj, originalName) {

		var name = nameObj.get("text");
		var value = valueObj.get("text");

		if (name === null || name === "") return;

		var items = dataSource.get("json");

		if (items === null) return;

		var index = -1;
		
		for (var i = 0; i < items.length; i++) {

			if (items[i].AttributeName === name) {
				index = i;
				break;
			}
		}

		if (index >= 0) {

			dataSource.remove(index);

			dataSource.insert({ "AttributeName": name, "AttributeValue": value, "ItemId": null }, index);
		}
		else
		{
			for (var a = 0; a < items.length; a++) {

				if (items[a].AttributeName === originalName) {
					index = a;
					break;
				}
			}
			if (index >= 0) {

				dataSource.remove(index);

				dataSource.insert({ "AttributeName": name, "AttributeValue": value, "ItemId": null }, index);
			}
		}
	};

	var removeDataSourceItem = function(dataSource, nameObj) {

		var name = nameObj.get("text");

		if (name === null) return;

		var items = dataSource.get("json");

        if (items === null || items === undefined) return;
        
		var index = -1;

		for (var i = 0; i < items.length; i++) {

			var item = items[i];

			if (item.AttributeName === name) {

				index = i;
				break;
			}
		}
		if (index >= 0) {
			dataSource.remove(index);
		}

	};

	var InsertLinkViaTreeDialog = Sitecore.Definitions.App.extend({
		initialized: function() {

			$that = this;

			setJsonDataSourceInitialValues = function(ds, field) {
				//get field values into collection
				var totalSet = field.get("text");
				if (totalSet === "" || totalSet === null) return;

				var fieldGroups = totalSet.split(",");

				for (var i = 0; i < fieldGroups.length; i++) {

					var fieldSet = fieldGroups[i];
					var attributes = fieldSet.split("|");
					var name = attributes[0];
					var value = attributes[1];

					ds.add(
						{
							"AttributeName": name,
							"AttributeValue": value,
							"ItemId": null
						}
					);
				}
			};

            setJsonDataSourceInitialValues(this.CustomAttributesDataSource, this.CustomAttributes);
            setJsonDataSourceInitialValues(this.QueryStringParametersDataSource, this.QueryStringParameters);


            if (this.UseDisplayNameCheckBox.get("text") === "true") {
                this.UseDisplayNameCheckBox.set("isChecked", true);
            } else {
                this.UseDisplayNameCheckBox.set("isChecked", false);
            }

			if (this.ForceSecureLinkCheckBox.get("text") === "true") {
                this.ForceSecureLinkCheckBox.set("isChecked", true);
            } else {
                this.ForceSecureLinkCheckBox.set("isChecked", false);
            }

            if (this.OpenNewWindowCheckBox.get("text") === "true") {
                this.OpenNewWindowCheckBox.set("isChecked", true);
            } else {
                this.OpenNewWindowCheckBox.set("isChecked", false);
            }

            if (this.NoReferrerCheckBox.get("text") === "true")
            {
				this.NoReferrerCheckBox.set("isChecked", true);
            } else {
				this.NoReferrerCheckBox.set("isChecked", false);
            }

            if (this.NoFollowCheckBox.get("text") === "true")
            {
				this.NoFollowCheckBox.set("isChecked", true);
            } else {
				this.NoFollowCheckBox.set("isChecked", false);
			}

            if (this.NoIndexCheckBox.get("text") === "true")
            {
				this.NoIndexCheckBox.set("isChecked", true);
            } else {
				this.NoIndexCheckBox.set("isChecked", false);
			}			

			this.CustomAttributesListControl.on("change:selectedItemId",
				function() {

					var activeRow = this.CustomAttributesListControl.get("selectedItem");

					if (activeRow === "" || activeRow === null) return;

					var name = activeRow.get("AttributeName");
					var value = activeRow.get("AttributeValue");

					__activeCustomAttribute = name;

					this.CustomAttributeName.set("text", name);
					this.CustomAttributeValue.set("text", value);

				},
				this);

			this.QueryStringParametersListControl.on("change:selectedItemId",
				function() {

					var activeRow = this.QueryStringParametersListControl.get("selectedItem");

					if (activeRow === "" || activeRow === null) return;

					var name = activeRow.get("AttributeName");
					var value = activeRow.get("AttributeValue");

                    __activeQueryStringParameter = name;

					this.QueryStringName.set("text", name);
					this.QueryStringValue.set("text", value);
				},
                this);

			this.TreeView.on("change:selectedItemId",
				function() {
                    if (this.__firstTimeBound) {
                        this.__firstTimeBound = false;
                        this.initializeItemRootNode();
                    }

                    var selectedItemId = this.TreeView.get("selectedItemId");                   

                    if (this.__rootNode && selectedItemId !== this.__rootNode) {
                        var options = this.LinkTypes.get("items");

                        if (options === null || options === undefined) return;

                        for (var i = 0; i < options.length; i++) {
                            if (options[i].itemId === linkTypeOptions.internal) {

                                this.LinkTypes.set("selectedItem", options[i]);
                                break;
                            }
                        }

                        var linkDisplayText = this.LinkDisplayText.get("text");
                        if (linkDisplayText && (linkDisplayText === "" || linkDisplayText === null)) {
                            this.enableUseDisplayName();                            
                        }
                    }
				},
				this);

			this.LinkTypesDataSource.on("change:items",
				function() {
					if (this.__linkTypesFirstTimeBound) {
                        this.__linkTypesFirstTimeBound = false;
						var local = this;
						var targets = this.LinkTypesLoadedItem.get("text").split(',');
						
                        targets.forEach(function (target) {

                            var options = local.LinkTypes.get("items");

                            if (options === null || options === undefined) return;

                            if (target === undefined || target === "") {
                                for (var i = 0; i < options.length; i++) {
                                    if (options[i].itemId === linkTypeOptions.internal) {
                                        local.LinkTypes.set("selectedItem", options[i]);
                                        local.hidePhoneMasks();
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                for (var l = 0; l < options.length; l++) {
                                    if (options[l].itemId === target) {
                                        local.LinkTypes.set("selectedItem", options[l]);
                                        break;
                                    }
                                }
                            }
						});
					}
				},
                this);

            this.PhoneMasksDataSource.on("change:items",
                function () {
                    if (this.__phoneMasksFirstTimeBound) {
                        this.__phoneMasksFirstTimeBound = false;
                        var local = this;
                        var targets = this.PhoneMasksLoadedItem.get("text").split(',');

                        targets.forEach(function (target) {

                            var options = local.PhoneMasks.get("items");

                            if (target === null || target === undefined) return;
                            if (options === null || options === undefined) return;

                            for (var i = 0; i < options.length; i++) {
                                if (options[i].itemId === target) {
                                    local.PhoneMasks.set("selectedItem", options[i]);
                                    break;
                                }
                            }
                            
                        });
                    }
                },
                this);

            this.PhoneMasks.on("change:selectedItem",
                function () {

                    if (this.__phoneMasksInitializeEvent) {
                        this.__phoneMasksInitializeEvent = false;

                        //For some reason the RootItem appears in the list, we need to make it unselectable
                        var items = this.PhoneMasks.get("items");
                        items.splice(0, 1);
                        this.PhoneMasks.set("items", items.slice());

                        return;
                    }
                    else {
                       
                        var selectedItem = this.PhoneMasks.get("selectedItem");

                        if (!selectedItem) return;

                        this.PhoneMasksLoadedItem.set("text", selectedItem.itemId);
                    }
                },
                this);

            this.LinkTypes.on("change:selectedItem",
                function () {
                    if (this.__linkTypesInitializeEvent === true) {
                        this.__linkTypesInitializeEvent = false;

                        this.initializeItemRootNode();

                        return;
                    }
                    else {

                        var selectedItem = this.LinkTypes.get("selectedItem");
                        if (!selectedItem) return;

                        if (this.LinkTypesLoadedItem.get("text") === "" && selectedItem.itemId === linkTypeOptions.external) {
                            this.enableForceSecureLinks();
                            this.enableOpenNewWindow();
                        }
                        if (this.LinkTypesLoadedItem.get("text") === "" && selectedItem.itemId === linkTypeOptions.internal) {
                            this.disableForceSecureLinks();
                            this.disableOpenNewWindow();
                        }

                        if (selectedItem.itemId !== linkTypeOptions.external) {
                            this.hideForceSecureLinks();                            
                        }
                        if (selectedItem.itemId !== linkTypeOptions.internal) {

                            this.resetToRootItemNode();
                            this.showCustomLink();                            
                            this.hideUseDisplayName();
                            this.showDisplayText();
                        }
                        else {
                            this.hideCustomLink();
                            this.hideForceSecureLinks();
                            this.showUseDisplayName();
                            this.showOpenNewWindow();   

                            if (this.UseDisplayNameCheckBox.get("isChecked") === true) {
                                this.hideDisplayText();
                            } else {
                                this.showDisplayText();                                
                            }
                        }

                        if (selectedItem.itemId === linkTypeOptions.phone) {
                            this.showPhoneMasks();
                            this.hideOpenNewWindow();
                        } else {
                            this.hidePhoneMasks();
                        }

                        if (selectedItem.itemId === linkTypeOptions.email) {
                            this.hideOpenNewWindow();
                        }

                        if (selectedItem.itemId === linkTypeOptions.external) {
                            this.showOpenNewWindow();
                            this.showForceSecureLinks();      
                        }
                        
                    }
                },
                this);
            this.UseDisplayNameCheckBox.on("change:isChecked",
                function () {

                    if (this.UseDisplayNameCheckBox.get("isChecked") === true) {
                        this.UseDisplayNameCheckBox.set("text", "true");
                        this.hideDisplayText();

                    } else {
                        this.UseDisplayNameCheckBox.set("text", "false");
                        this.showDisplayText();
                    }
                },
                this);

            this.NoFollowCheckBox.on("change:isChecked",
				function() {

					if (this.NoFollowCheckBox.get("isChecked") === true) {
						this.NoFollowCheckBox.set("text", "true");
					} else {
						this.NoFollowCheckBox.set("text", "false");
					}
				},
				this);

			this.NoIndexCheckBox.on("change:isChecked",
				function() {

					if (this.NoIndexCheckBox.get("isChecked") === true) {
						this.NoIndexCheckBox.set("text", "true");
					} else {
						this.NoIndexCheckBox.set("text", "false");
					}

				},
				this);
			this.NoReferrerCheckBox.on("change:isChecked",
				function() {

					if (this.NoReferrerCheckBox.get("isChecked") === true) {
						this.NoReferrerCheckBox.set("text", "true");
					} else {
						this.NoReferrerCheckBox.set("text", "false");
					}

				},
				this);
            this.OpenNewWindowCheckBox.on("change:isChecked",
                function () {

                    if (this.OpenNewWindowCheckBox.get("isChecked") === true) {
                        this.OpenNewWindowCheckBox.set("text", "true");
                    } else {
                        this.OpenNewWindowCheckBox.set("text", "false");
                    }

                },
                this);

			this.ForceSecureLinkCheckBox.on("change:isChecked",
				function() {

                    if (this.ForceSecureLinkCheckBox.get("isChecked") === true) {
                        this.ForceSecureLinkCheckBox.set("text", "true");
					} else {
                        this.ForceSecureLinkCheckBox.set("text", "false");
					}

				},
                this);     
            
		},

      
        initializeItemRootNode: function () {
            if (!this.__rootNodeInitializedEvent) {
                this.__rootNodeInitializedEvent = true;

                this.__rootPath = this.RootItem.get("text");

                if (this.__rootPath !== "") {
                    var arr = this.__rootPath.split("/");
                    this.__rootNode = arr[arr.length - 1];
                }
            }
        },
        resetToRootItemNode: function () {

            if (this.__rootNode !== "") {
                this.TreeView.viewModel.pathToLoad(this.__rootNode);
                this.TreeView.viewModel.loadKeyPath();
            }
        },
        enableUseDisplayName: function () {
            this.UseDisplayNameCheckBox.set("isChecked", true);
            this.UseDisplayNameCheckBox.set("text", "true");
        },
        disableUseDisplayName: function () {
            this.UseDisplayNameCheckBox.set("isChecked", false);
            this.UseDisplayNameCheckBox.set("text", "false");
        },
        enableForceSecureLinks: function () {
            this.ForceSecureLinkCheckBox.set("isChecked", true);
            this.ForceSecureLinkCheckBox.set("text", "true");
        },
        disableForceSecureLinks: function () {
            this.ForceSecureLinkCheckBox.set("isChecked", false);
            this.ForceSecureLinkCheckBox.set("text", "false");
        },
        enableOpenNewWindow: function () {
            this.OpenNewWindowCheckBox.set("isChecked", true);
            this.OpenNewWindowCheckBox.set("text", "true");
        },
        disableOpenNewWindow: function () {
            this.OpenNewWindowCheckBox.set("isChecked", false);
            this.OpenNewWindowCheckBox.set("text", "false");
        },

        hideDisplayText: function () {
            this.LinkDisplayText.set("isVisible", false);
            this.LinkDisplayTextLbl.set("isVisible", false);
        },
        showDisplayText: function () {
            this.LinkDisplayTextLbl.set("isVisible", true);
            this.LinkDisplayText.set("isVisible", true);
        },
        showUseDisplayName: function () {
            this.UseDisplayNameCheckBox.set("isVisible", true);
            this.UseDisplayNameRow.set("isVisible", true);
        },        
        hideUseDisplayName: function () {
            this.UseDisplayNameCheckBox.set("isVisible", false);
            this.UseDisplayNameRow.set("isVisible", false);
        },
        showPhoneMasks: function () {
            this.PhoneMasksLbl.set("isVisible", true);
            this.PhoneMasks.set("isVisible", true);
            this.PhoneBehaviorContainer.set("isVisible", true);
        },
        hidePhoneMasks: function () {

            this.PhoneBehaviorContainer.set("isVisible", false);
            this.PhoneMasksLbl.set("isVisible", false);
            this.PhoneMasks.set("isVisible", false);
        },
        hideForceSecureLinks: function () {
            this.ForceSecureLinkCheckBox.set("isVisible", false);
            this.ForceSecureLinkLbl.set("isVisible", false);
        },
        showForceSecureLinks: function () {
            this.ForceSecureLinkCheckBox.set("isVisible", true);
            this.ForceSecureLinkLbl.set("isVisible", true);
        },
        showOpenNewWindow: function () {
            this.OpenNewWindowCheckBox.set("isVisible", true);
            this.OpenNewWindowLbl.set("isVisible", true);
        },
        hideOpenNewWindow: function () {
            this.OpenNewWindowCheckBox.set("isVisible", false);
            this.OpenNewWindowLbl.set("isVisible", false);
        },

        showCustomLink: function () {
            this.CustomLinkRow.set("isVisible", true);
            
        },
        hideCustomLink: function () {
            this.CustomLinkRow.set("isVisible", false);
            
        },

		addCustomAttribute: function() {

			addDataSourceItem(this.CustomAttributesDataSource, this.CustomAttributeName, this.CustomAttributeValue);
		},
		addQueryStringParameter: function() {

			addDataSourceItem(this.QueryStringParametersDataSource, this.QueryStringName, this.QueryStringValue);
		},

		updateCustomAttribute: function() {

			updateDataSourceItem(
				this.CustomAttributesDataSource,
				this.CustomAttributeName,
				this.CustomAttributeValue,
				__activeCustomAttribute);

		},
		updateQueryStringParameter: function () {

			updateDataSourceItem(
				this.QueryStringParametersDataSource,
				this.QueryStringName,
				this.QueryStringValue,
                __activeQueryStringParameter);
        },



		removeCustomAttribute: function() {
			removeDataSourceItem(this.CustomAttributesDataSource, this.CustomAttributeName);
		},
		removeQueryStringParameter: function() {
			removeDataSourceItem(this.QueryStringParametersDataSource, this.QueryStringName);
		},

		insertInternalLinkResult: function() {

			var joinDataSourceStrings = function(data) {

				var json = data.get("json");

				if (json === null) return "";

				var pairs = [];

				for (var i = 0; i < json.length; i++) {

					var obj = json[i];

					pairs.push(obj.AttributeName + "|" + obj.AttributeValue);
				}

				return pairs.join(',');

            };

            
            var selectedLinkTypeId = this.LinkTypes.get("selectedItemId");
            this.LinkTypesLoadedItem.set("text", selectedLinkTypeId);

			this.QueryStringParameters.set("text", joinDataSourceStrings(this.QueryStringParametersDataSource));
			this.CustomAttributes.set("text", joinDataSourceStrings(this.CustomAttributesDataSource));


            var linkDisplayTextValue = this.LinkDisplayText,

                linkTypesLoadedItem = this.LinkTypesLoadedItem.get("text"),
                linkTypesLoadedValue = this.LinkTypesLoadedValue,

                phoneMasksLoadedItem = this.PhoneMasksLoadedItem.get("text"),                

                customAttributes = this.CustomAttributes,
                queryStringParameters = this.QueryStringParameters,

                useDisplayNameCheckBox = this.UseDisplayNameCheckBox,
                openNewWindowCheckBox = this.OpenNewWindowCheckBox,
                forceSecureLinkCheckBox = this.ForceSecureLinkCheckBox,
                noReferrerCheckBox = this.NoReferrerCheckBox,
                noFollowCheckBox = this.NoFollowCheckBox,
                noIndexCheckBox = this.NoIndexCheckBox,

                treeViewControl = this.TreeView,

                cssClass = this.CssClass,
                customLinkValue = this.CustomLink,

                elementId = this.ElementId,
                textBeforeLink = this.TextBeforeLink,
                textAfterLink = this.TextAfterLink,

                anchorTarget = this.AnchorTarget,

                template = {
                    textBeforeLink: "textBeforeLink",
                    linkDisplayText: "linkDisplayText",
                    textAfterLink: "textAfterLink",
                    elementId: "elementId",
                    anchorTarget: "anchorTarget",
                    linkType: "linkType",
                    linkTypesLoadedItem: "linkTypesLoadedItem",
                    phoneMasksLoadedItem: "phoneMasksLoadedItem",
                    url: "url",
                    customLink: "customLink",
                    cssClass: "cssClass",
                    id: "itemId",
                    customAttributes: "customAttributes",
                    queryStringParameters: "queryStringParameters",
                    noFollowCheckBox: "noFollowCheckBox",
                    noIndexCheckBox: "noIndexCheckBox",
                    noReferrerCheckBox: "noReferrerCheckBox",
                    openNewWindowCheckBox: "openNewWindowCheckBox",
                    forceSecureLinkCheckBox: "forceSecureLinkCheckBox",
                    useDisplayNameCheckBox: "useDisplayNameCheckBox"
                },
                path,
                generateTemplate = function (template) {

                    var output = "<ultralink ";

                    for (var key in template) {
                        if (template.hasOwnProperty(key)) {
                            output += key + '="<%=' + template[key] + '%>" ';
                        }
                    }
                    output = output + " />";

                    return output;
                },
				htmlEncode = function(str) {
					return str.toString().replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
						.replace(/"/g, '&quot;');
				};


            if (treeViewControl) {

                var selectedNode = treeViewControl.get("selectedNode");

                if (selectedNode) {

                    if ("rawItem" in selectedNode && treeViewControl.get("selectedNode").rawItem["$path"]) {
                        path = selectedNode.rawItem["$path"];
                    } else {
                        path = treeViewControl.get("selectedItemPath");
                    }                    
                }
            } 

            var actualSitecoreLinkText = customLinkValue.get("text");
            var customUrlSet = linkTypesLoadedItem !== linkTypeOptions.internal;
            var idVal = null;

            if (customUrlSet) {

                if (linkTypesLoadedItem === linkTypeOptions.phone) {
                    linkTypesLoadedValue.set("text", "phone");
                    actualSitecoreLinkText = "tel:+" + actualSitecoreLinkText;
                }
                if (linkTypesLoadedItem === linkTypeOptions.email) {
                    linkTypesLoadedValue.set("text", "email");
                    actualSitecoreLinkText = "mailTo:" + actualSitecoreLinkText;
                }
                if (linkTypesLoadedItem === linkTypeOptions.external) {
                    linkTypesLoadedValue.set("text", "external");
                }
            }
            else {
                idVal = treeViewControl.get("selectedItemId");                
                linkTypesLoadedValue.set("text", "internal");
                actualSitecoreLinkText = path;
            }

            var templateStr = generateTemplate(template);

			var itemLink = _.template(templateStr,
				{
                    itemId: idVal,
                    path: path,
                    url: htmlEncode(actualSitecoreLinkText),

                    linkType: htmlEncode(linkTypesLoadedValue.get("text")),

                    textBeforeLink: htmlEncode(textBeforeLink.get("text")),
                    linkDisplayText: htmlEncode(linkDisplayTextValue.get("text")),
                    textAfterLink: htmlEncode(textAfterLink.get("text")),
                    cssClass: htmlEncode(cssClass.get("text")),                    
                    anchorTarget: htmlEncode(anchorTarget.get("text")),                    
					customAttributes: htmlEncode(customAttributes.get("text")),
                    queryStringParameters: htmlEncode(queryStringParameters.get("text")),

                    noFollowCheckBox: noFollowCheckBox.get("isChecked"),
					noReferrerCheckBox: noReferrerCheckBox.get("isChecked"),
					noIndexCheckBox: noIndexCheckBox.get("isChecked"),
                    forceSecureLinkCheckBox: forceSecureLinkCheckBox.get("isChecked"),
                    openNewWindowCheckBox: openNewWindowCheckBox.get("isChecked"),
                    useDisplayNameCheckBox: useDisplayNameCheckBox.get("isChecked"),                    

					elementId: htmlEncode(elementId.get("text")),
                    customLink: htmlEncode(customLinkValue.get("text")),                    

                    linkTypesLoadedItem: htmlEncode(linkTypesLoadedItem),
                    phoneMasksLoadedItem: htmlEncode(phoneMasksLoadedItem)
				});

			return this.closeDialog(itemLink);
		},
		__activeCustomAttribute: "",
        __activeQueryStringParameter: "",
		__firstTimeBound: true,
        __firstTimeSelected: true,
        __rootNodeInitializedEvent: false,
        __rootNode: "",
        __rootPath: "",
		__linkTypesFirstTimeBound: true,
        __linkTypesInitializeEvent: true,
        __phoneMasksFirstTimeBound: true,
        __phoneMasksInitializeEvent: true,
		__activeBrowserTargetId: '{6CC56D72-0290-44EB-98DE-779BEB3303A8}',

});

	return InsertLinkViaTreeDialog;
});