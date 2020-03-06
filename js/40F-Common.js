var FF = FF || {};

// Always use this method for logging. That way we can always implemented more advanced techniques later on
FF.log = function (msg) {
    console.log(msg);
};

FF.parseJSON = function (json) {
    var retval;

    if (typeof (json) === "string") {
        retval = JSON.parse(json);
    } else {
        // it's probably already an object
        retval = json;
    }
    return retval;
};

FF.isNullOrEmptyString = function (s) {
    return s === "" || s === null;
};

FF.addLoader = function (targetSelector) {
    $(targetSelector).prepend("<div class=\"ff-loading\"></div>");
};
FF.removeLoader = function (targetSelector) {
    $(targetSelector + " .ff-loading").remove();
};

FF.constants = {
    animationSpeed: 500
};

FF.confirm = function (message) {
    // currently the easy javascreipt way, but probably a nicer method needed in the future
    return confirm(message);
};

FF.isVisible = function (selector) {
    return $(selector).has("[hidden]").length === 0;
};
FF.hide = function (selector, callback) {
    $(selector).attr("hidden", "hidden");

    // maybe we need to call a function
    if (typeof (callback) === "function") {
        callback();
    }
};
FF.show = function (selector, callback) {
    // remove the hidden attribute
    $(selector).removeAttr("hidden");
    // maybe we need to call a function
    if (typeof (callback) === "function") {
        callback();
    }
};

FF.hideNshow = function (hideSelector, showSelector, callback) {
    FF.hide(hideSelector, function () { FF.show(showSelector, callback); });
};

//https://stackoverflow.com/questions/9234830/how-to-hide-a-option-in-a-select-menu-with-css
jQuery.fn.toggleOption = function (show) {
    $(this).each(function () {
        if (show) {
            FF.show(this);
            if ($(this).parent("span.toggleOption").length)
                $(this).unwrap();
        } else {
            FF.hide(this);
            if ($(this).parent("span.toggleOption").length === 0)
                $(this).wrap('<span class="toggleOption" style="display: none;" />');
        }
    });
};

FF.hideOption = function (selector, callback) {
    $(selector).toggleOption(false);
    // maybe we need to call a function
    if (typeof (callback) === "function") {
        callback();
    }
};
FF.showOption = function (selector, callback) {
    $(selector).toggleOption(true);
    // maybe we need to call a function
    if (typeof (callback) === "function") {
        callback();
    }
};

FF.hideNshowOption = function (hideSelector, showSelector, callback) {
    FF.hideOption(hideSelector, function () { FF.showOption(showSelector, callback); });
};


FF.resetForm = function (formContainerSelector) {
    $(formContainerSelector +
        " input, " +
        formContainerSelector +
        " select, " +
        formContainerSelector +
        " textarea"
    )
        .each(function (ix, elm) {
            try {
                $(elm).val(function () {
                    return this.defaultValue;
                });
            } catch (e) {
                FF.log("FF.resetForm error");
            }
        });

    return;
};

FF.getFormJson = function (containerSelector) {
    var obj = FF.getFormFieldsObject(containerSelector);
    var json = JSON.stringify(obj);
    FF.log("built json for " + containerSelector + ": " + json);
    return json;

};

FF.getFormFieldsObject = function (containerSelector) {
    var obj = {}; // create new object

    //FF.log("getFormFieldsObject started for: " + containerSelector);

    // we need to keep track of the items we already parsed
    var parsedItems = {};

    // TODO: idea: first collect all name-value pairs from all input/select/textarea controls
    //             then parse them into the object

    // append each input element to the formData object
    $(containerSelector +
        " input, " +
        containerSelector +
        " select, " +
        containerSelector +
        " textarea"
    )
        .each(function (index, element) {
            // if it's a radiobutton: only add it if it's selected
            var type = $(element).attr("type");
            if (type === "radio" && !$(element).is(':checked')) {
                // it's an unchecked radio: nothing to do
                return;
            }
            if (type === "checkbox" && !$(element).is(':checked')) {
                // it's an unchecked checkbox: nothing to do
                return;
            }

            //$("input:radio[name='choices']:checked").val();

            // try the name first, otherwise get the id
            var name = $(element).attr("name");
            if (typeof (name) === "undefined" || name === null || name === "") {
                name = $(element).attr("id");
            }
            var val = null;
            if ($(element).hasClass("hasDatepicker")) {
                // for a datepicker control we need to get the date value
                //val = $(element).datepicker("getDate");
                val = $.datepicker.parseDate($(element).datepicker("option", "dateFormat"), $(element).val());
            } else {
                val = $(element).val();
            }

            //FF.log("append formdata for name: " + name);
            if (typeof name === "undefined" || name === null || name === "") {
                // no name: nothing to do
                return;
            }


            // if the id doesn't contain a dot, then we can just add the field to the object
            if (name.indexOf(".") < 0) {
                //FF.log("append formdata name: " + name + ", value: " + val);
                obj[name] = val;
            } else {
                //FF.log("formdata " + name + " seems to be a collection");
                // the name contains a dot, it is part of a collection, so we need to decipher the name and 
                // build the collection of objects with their properties
                // The name of the field will be built like this
                // CollectionPropertyName.Key.PropertyName
                // whereas key is some kind of id of the item
                // the part before the PropertyName will serve as an identifier
                // all fields for the same item will have an identical identifier
                var identifier = name.substr(0, name.lastIndexOf("."));
                // split the nameparts
                var nameParts = name.split(".");
                // get and keep the collectionName
                var collectionName = nameParts[0];
                // get and remove the PropertyName
                var propertyName = nameParts.pop();

                //FF.log("formdata " + name + " is property " + propertyName + " for item with key " + identifier + " of collection " + collectionName);

                // does the collection object already exists?
                // if not, create it
                if (typeof (obj[collectionName]) === "undefined") {
                    obj[collectionName] = [];
                }

                // have we already parsed another property for this identifier?
                if (typeof (parsedItems[collectionName]) === "undefined") {
                    parsedItems[collectionName] = [];
                }

                var tmpIndex = -1;
                $.each(parsedItems[collectionName],
                    function (i, str) {
                        if (str === identifier) {
                            tmpIndex = i;
                        }
                    });

                var itemIndex = tmpIndex;
                //var itemIndex = parsedItems[collectionName].findIndex(function (currentVal) { return currentVal === identifier; });

                //FF.log("ItemIndex found: " + itemIndex.toString());

                // if itemIndex equals -1 then we don't have it yet
                if (itemIndex < 0) {
                    var newObj = {}; // create the new object for the property
                    parsedItems[collectionName].push(identifier); // keep track of what we already have
                    obj[collectionName].push(newObj); // add the object to the collection
                    itemIndex = obj[collectionName].length - 1; // keep the new itemIndex
                    //FF.log("ItemIndex added: " + itemIndex.toString());
                }

                // now, add the property
                obj[collectionName][itemIndex][propertyName] = val;
                //FF.log("form item is now: " + JSON.stringify(obj));
                //FF.log("collection item is now: " + JSON.stringify(obj[collectionName][itemIndex]));
                //FF.log("parseditems is now: " + JSON.stringify(parsedItems));
            }

        });

    FF.log("built object for " + containerSelector + ": " + JSON.stringify(obj));
    return obj;
};

FF.getService = function (path, controllername, servicemoduleId) {
    var ffservice = {
        path: path, // ModuleFoldername from routemapper
        framework: $.ServicesFramework(servicemoduleId) //$.dnnSF(servicemoduleId)
    };
    ffservice.baseUrl = ffservice.framework.getServiceRoot(ffservice.path) + controllername + "/";

    return ffservice;
};

FF.clearContainer = function (targetSelector, addLoader) {
    $(targetSelector).html("");
    if (typeof (addLoader) === "boolean" && addLoader) {
        FF.addLoader(targetSelector);
    }
};

FF.getData = function (service, method, isLoadingTargetSelector, onDone, onFail, onAlways) {
    FF.log("getData " + method);

    $(isLoadingTargetSelector).addClass("ff-loading");

    $.ajax({
        url: service.baseUrl + method,
        beforeSend: service.framework.setModuleHeaders,
        dataType: "html"
    }).done(function (data) {
        // maybe we need to call a function
        if (typeof (onDone) === "function") {
            onDone(data);
        }
    }).fail(function (data) {
        // maybe we need to call a function
        if (typeof (onFail) === "function") {
            onFail(data);
        }
    }).always(function (data) {
        $(isLoadingTargetSelector).removeClass("ff-loading");

        // maybe we need to call a function
        if (typeof (onAlways) === "function") {
            onAlways(data);
        }

    });
};

FF.getForm = function (service, method, targetSelector, onDone, onFail, onAlways) {
    FF.log("getForm " + method);

    // clear the target
    FF.clearContainer(targetSelector, true);

    // add a loader
    //FF.addLoader(targetSelector);

    var loaderStart = performance.now();

    $.ajax({
        url: service.baseUrl + method,
        beforeSend: service.framework.setModuleHeaders,
        dataType: "html"
    }).done(function (data) {
        // maybe we need to insert the data into an element
        if (typeof (targetSelector) === "string") {
            if (data) {
                // put the form in its placeholder, use append to leave the loader there
                $(targetSelector).append(data);
            } else {
                // something's fishy
                FF.log("getForm returned no data");
            }
        }
        // maybe we need to call a function
        if (typeof (onDone) === "function") {
            onDone(data);
        }
    }).fail(function (data) {
        // maybe we need to call a function
        if (typeof (onFail) === "function") {
            onFail(data);
        }
    }).always(function (data) {
        //FF.removeLoader(targetSelector);
        // make sure the loader is shown at least 500 ms
        setTimeout(function () { FF.removeLoader(targetSelector); },
            500 - (performance.now() - loaderStart)
        );

        // maybe we need to call a function
        if (typeof (onAlways) === "function") {
            onAlways(data);
        }

    });
};
//FF.getForm = function (service, method, targetSelector) {
//    return FF.getForm(service, method, targetSelector, null, null);

//    //$.ajax({
//    //    url: service.baseUrl + method,
//    //    beforeSend: service.framework.setModuleHeaders,
//    //    dataType: "html"
//    //}).done(function (data) {
//    //    // maybe we need to insert the data into an element
//    //    if (typeof (targetSelector) == "string") {
//    //        if (data) {
//    //            // put the form in its placeholder
//    //            $(targetSelector).html(data);
//    //        }
//    //    }
//    //}).fail(function (data) {
//    //}).always(function (data) {
//    //});
//}

FF.postForm = function (service, method, formContainerSelector, onDone, onFail) {
    var obj = FF.getFormJson(formContainerSelector);
    return FF.postFormJson(service, method, obj, onDone, onFail);
};
FF.postFormFieldsObject = function (service, method, formFieldsObject, onDone, onFail) {
    var json = JSON.stringify(formFieldsObject);
    return FF.postFormJson(service, method, json, onDone, onFail);
};
FF.postFormJson = function (service, method, formJson, onDone, onFail) {
    $.ajax({
        url: service.baseUrl + method,
        beforeSend: service.framework.setModuleHeaders,
        data: formJson,
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        processData: false,
        dataType: "html"
    })
        .done(function (data) {
            FF.log("Form data saved");
            // maybe we need to call a function
            if (typeof (onDone) === "function") {
                onDone(data);
            }
        })
        .fail(function (jqXHR, textStatus) {
            // maybe we need to call a function
            if (typeof (onFail) === "function") {
                onFail(jqXHR.responseText);
            }
        })
        .always(function (data) {
        });
};

FF.postMethod = function (service, method, onDone, onFail, onAlways) {
    FF.log("About to post method " + method);
    $.ajax({
        url: service.baseUrl + method,
        beforeSend: service.framework.setModuleHeaders,
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        processData: false,
        dataType: "html"
    })
        .done(function (data) {
            FF.log("Posted to method: " + data);
            // maybe we need to call a function
            if (typeof (onDone) === "function") {
                onDone(data);
            }
        })
        .fail(function (jqXHR, textStatus) {
            // maybe we need to call a function
            if (typeof (onFail) === "function") {
                onFail(jqXHR.responseText);
            }
        })
        .always(function (data) {
            // maybe we need to call a function
            if (typeof (onAlways) === "function") {
                onAlways(data);
            }
        });
};


FF.postFiles = function (service, method, fileInputSelector, idSelector, onDone, onFail, onAlways) {
    FF.log("FF.postfiles called");
    // because a form reset also triggers the change event for file input fields, we're checking if there are any files:
    if ($(fileInputSelector)[0].files.length === 0) {
        return;
    }

    var filesdata = new FormData(); // create new formdata object
    // put the files in the data object
    $.each($(fileInputSelector)[0].files,
        function (i, file) {
            filesdata.append('afbeelding', file);
            //data.append('file-' + i, file);
        });
    // add the id
    filesdata.append("Id", $(idSelector).val());

    FF.log("About to upload " + $(fileInputSelector)[0].files.length.toString() + " files.");

    $.ajax({
        url: service.baseUrl + method,
        beforeSend: service.framework.setModuleHeaders,
        data: filesdata,
        cache: false,
        contentType: false,
        processData: false,
        type: 'POST'
    })
        .done(function (data) {
            FF.log("Files uploaded");
            // maybe we need to call a function
            if (typeof (onDone) === "function") {
                onDone(data);
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            // textStatus can be: "timeout", "error", "abort", and "parsererror"
            // errorThrown receives the textual portion of the HTTP status, such as "Not Found" or "Internal Server Error." 

            // maybe we need to call a function
            if (typeof (onFail) === "function") {
                onFail(jqXHR.responseText);
            }
        })
        .always(function (data) {
            // maybe we need to call a function
            if (typeof (onAlways) === "function") {
                onAlways(data);
            }
        });
};

FF.disableControl = function (selector) {
    if ($(selector).is("select,input[type='radio'],input[type='checkbox']")) {
        //FF.log("Disabling select/check/radio " + selector);
        $(selector).attr("disabled", "");
    } else if ($(selector).is(".hasDatepicker")) {
        //FF.log("Disabling datepicker " + selector);
        $(selector).datepicker("option", "disabled", true);
    } else if ($(selector).is("input,textarea")) {
        //FF.log("Disabling input/textarea " + selector);
        $(selector).attr("readonly", "");
    }
};

FF.setEnabledButton = function (selector, enabled) {
    if (enabled) {
        FF.enableButton(selector);
    } else {
        FF.disableButton(selector);
    }
};
FF.disableButton = function (selector) {
    $(selector).addClass("disabled");
    $(selector).attr("disabled", "disabled");
};
FF.enableButton = function (selector) {
    $(selector).removeClass("disabled");
    $(selector).removeAttr("disabled");
};

FF.scrollTo = function (selector) {
    $('html, body').animate({
        scrollTop: $(selector).offset().top - 100 // minus 100 to prevent the target to be too tight to the top
    },
        FF.constants.animationSpeed);
};

FF.addTooltipHandlers = function () {
    $('[data-toggle="tooltip"]')
        .addClass("ffShowHelp")
        .tooltip({ trigger: "click" });
};