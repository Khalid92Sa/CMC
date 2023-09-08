/// <summary>
/// apiHelper class for define ajax call
/// </summary>
/// <method name="callService"> function take parameters important for the ajax call </method>
var APIHelper = {
    callService: function (type, url, data, options, contentType, dataType, successCallBackFunction, failureCallBackFunction) {
        try {
            //params class is including the parameters of ajax call
            var params = {
                cache: false,
                url: url,
                async: true,
                type: type,
                processData: false,
                data: data,
                contentType: contentType,
                dataType: dataType,
                success: successCallBackFunction,
                error: failureCallBackFunction
            };

            // options parameter is an array of headers to set the request of these headers, it has key and value.
            if (options !== null) {
                params.beforeSend = function (xhr) {
                    for (let i = 0; i < options.length; i++) {
                        xhr.setRequestHeader(options[i].key, options[i].value);
                    }
                };
            }

            //the ajax call
            $.ajax(params);
        }
        catch (e) {
            console.log(e);
        }
    },
    // #region httpGet
    /// <summary>
    /// HttpGet function for Get call
    /// </summary>
    /// <returns> successCallBackFunction if the call succeeded and failureCallBackFunction if there is an error with the call </returns>
    httpGet: function (url, data, optionHeaders, successCallBackFunction, failureCallBackFunction) {
        this.callService("GET", url, data, optionHeaders, "application/json; charset=utf-8", "json", successCallBackFunction, failureCallBackFunction);
    },
    // #endregion

    // #region httpPost
    /// <summary>
    /// HttpPost function for Post call
    /// </summary>
    /// <params name="contentType"> a boolean parameter, if it's value is true or null or undefined then set the contentType to be "application/json; charset=utf-8" else it's not set </method>
    /// <returns> successCallBackFunction if the call succeeded and failureCallBackFunction if there is an error with the call </returns>
    httpPost: function (url, data, optionHeaders, successCallBackFunction, failureCallBackFunction, contentType) {
        
        if (contentType === null || contentType || contentType === undefined) {
            contentType = "application/json; charset=utf-8";
        }
        this.callService("POST", url, data, optionHeaders, contentType, "json", successCallBackFunction, failureCallBackFunction);
    },
    // #endregion

    // #region httpPut
    /// <summary>
    /// HttpPut function for Put call
    /// </summary>
    /// <params name="contentType"> a boolean parameter, if it's value is true or null or undefined then set the contentType to be "application/json; charset=utf-8" else it's not set </method>
    /// <returns> successCallBackFunction if the call succeeded and failureCallBackFunction if there is an error with the call </returns>

    httpPut: function (url, data, optionHeaders, successCallBackFunction, failureCallBackFunction, contentType) {

        if (contentType === null || contentType || contentType === undefined) {
            contentType = "application/json; charset=utf-8";
        }
        this.callService("PUT", url, data, optionHeaders, contentType, "json", successCallBackFunction, failureCallBackFunction);
    },
    // #endregion

    // #region httpDelete
    /// <summary>
    /// HttpDelete function for Delete call
    /// </summary>
    /// <returns> successCallBackFunction if the call succeeded and failureCallBackFunction if there is an error with the call </returns>
    httpDelete: function (url, data, optionHeaders, successCallBackFunction, failureCallBackFunction) {
        this.callService("DELETE", url, data, optionHeaders, "application/json; charset=utf-8", "json", successCallBackFunction, failureCallBackFunction);
    }
    // #endregion
};

