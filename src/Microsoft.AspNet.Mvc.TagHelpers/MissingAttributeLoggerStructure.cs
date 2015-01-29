// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.TagHelpers
{
	internal class MissingAttributeLoggerStructure : ILoggerStructure
	{
		private readonly string _uniqueId;
	    private readonly IEnumerable<string> _missingAttributes;
	    
	    public MissingAttributeLoggerStructure(string uniqueId, IEnumerable<string> missingAttributes)
	    {
	        _uniqueId = uniqueId;
	        _missingAttributes = missingAttributes;
	    }
	    
	    string ILoggerStructure.Message
	    {
	        get
	        {
	            return "Tag Helper skipped due to missing required attributes.";
	        }
	    }
	
	    IEnumerable<KeyValuePair<string, object>> ILoggerStructure.GetValues()
	    {
	        return new Dictionary<string, object>
	        {
	            { "UniqueId", _uniqueId },
	            { "MissingAttributes", _missingAttributes }
	        };
	    }
	
	    string ILoggerStructure.Format()
	    {
	        return string.Format("Tag Helper unique ID: {0}, Missing attributes: {1}", _uniqueId, string.Join(",", _missingAttributes));
	    }
	}
}