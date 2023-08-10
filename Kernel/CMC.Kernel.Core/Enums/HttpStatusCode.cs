using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Kernel.Core.Enums
{
    public enum HttpStatusCode
    {
        None,

        Ok = 200,

        BadRequest = 400,
        /// <summary>
        /// Not authenticated
        /// </summary>
        NotAuthenticated = 401,                     // Not Authorized
        /// <summary>
        /// Not authorized
        /// </summary>
        NotAuthorized = 403,                        // Forbidden
        /// <summary>
        /// Not found
        /// </summary>
        NotFound = 404,
        /// <summary>
        /// The business rule violation
        /// </summary>
        BusinessRuleViolation = 422,                // Unprocessable Entity

    }
}
