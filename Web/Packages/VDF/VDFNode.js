var VDFNode = (function () {
    function VDFNode(primitiveValue, metadata) {
        this.listChildren = new List("VDFNode");
        this.mapChildren = new Dictionary("VDFNode", "VDFNode"); // this also holds Dictionaries' keys/values
        this.primitiveValue = primitiveValue;
        this.metadata = metadata;
    }
    VDFNode.prototype.SetListChild = function (index, value) {
        this.listChildren[index] = value;
        this[index] = value;
    };
    /*InsertListChild(index: number, value: any)
    {
        var oldItems = this.listChildren;
        for (var i = 0; i < oldItems.length; i++) // we need to first remove old values, so the slate is clean for manual re-adding/re-ordering
            delete this[i];
        for (var i = 0; i < oldItems.length + 1; i++) // now add them all back in, in the correct order
            this.AddListChild(i == 0 ? value : (i < index ? oldItems[i] : oldItems[i - 1]));
    }*/
    VDFNode.prototype.AddListChild = function (value) { this.SetListChild(this.listChildren.length, value); };
    VDFNode.prototype.SetMapChild = function (key, value) {
        this.mapChildren.Set(key, value);
        if (typeof key.primitiveValue == "string")
            this[key] = value;
    };
    VDFNode.prototype.toString = function () { return this.primitiveValue ? this.primitiveValue.toString() : ""; }; // helpful for debugging
    // saving
    // ==================
    VDFNode.PadString = function (unpaddedString) {
        var result = unpaddedString;
        if (result.StartsWith("<") || result.StartsWith("#"))
            result = "#" + result;
        if (result.EndsWith(">") || result.EndsWith("#"))
            result += "#";
        return result;
    };
    VDFNode.prototype.ToVDF = function (options, tabDepth) {
        if (options === void 0) { options = null; }
        if (tabDepth === void 0) { tabDepth = 0; }
        return this.ToVDF_InlinePart(options, tabDepth) + this.ToVDF_PoppedOutPart(options, tabDepth);
    };
    VDFNode.prototype.ToVDF_InlinePart = function (options, tabDepth, isKey) {
        if (options === void 0) { options = null; }
        if (tabDepth === void 0) { tabDepth = 0; }
        if (isKey === void 0) { isKey = false; }
        options = options || new VDFSaveOptions();
        var builder = new StringBuilder();
        if (options.useMetadata && this.metadata != null)
            builder.Append(this.metadata + ">");
        if (this.primitiveValue == null) {
            if (!this.isMap && this.mapChildren.Count == 0 && !this.isList && this.listChildren.Count == 0)
                builder.Append("null");
        }
        else if (typeof this.primitiveValue == "boolean")
            builder.Append(this.primitiveValue.toString().toLowerCase());
        else if (typeof this.primitiveValue == "string") {
            var unpaddedString = this.primitiveValue;
            // (the parser doesn't actually need '<<' and '>>' wrapped for single-line strings, but we do so for consistency)
            //var needsEscaping = unpaddedString.Contains("\"") || unpaddedString.Contains("'") || unpaddedString.Contains("\n") || unpaddedString.Contains("<<") || unpaddedString.Contains(">>");
            var needsEscaping = VDFNode.charsThatNeedEscaping_1_regex.test(unpaddedString);
            if (isKey)
                //needsEscaping = needsEscaping || unpaddedString.Contains("{") || unpaddedString.Contains("}") || unpaddedString.Contains("[") || unpaddedString.Contains("]") || unpaddedString.Contains(":");
                needsEscaping = needsEscaping || VDFNode.charsThatNeedEscaping_2_regex.test(unpaddedString);
            if (needsEscaping) {
                var literalStartMarkerString = "<<";
                var literalEndMarkerString = ">>";
                while (unpaddedString.Contains(literalStartMarkerString) || unpaddedString.Contains(literalEndMarkerString)) {
                    literalStartMarkerString += "<";
                    literalEndMarkerString += ">";
                }
                builder.Append((isKey ? "" : "\"") + literalStartMarkerString + VDFNode.PadString(unpaddedString) + literalEndMarkerString + (isKey ? "" : "\""));
            }
            else
                builder.Append((isKey ? "" : "\"") + unpaddedString + (isKey ? "" : "\""));
        }
        else if (VDF.GetIsTypePrimitive(VDF.GetTypeNameOfObject(this.primitiveValue)))
            builder.Append(options.useNumberTrimming && this.primitiveValue.toString().StartsWith("0.") ? this.primitiveValue.toString().substr(1) : this.primitiveValue);
        else
            builder.Append("\"" + this.primitiveValue + "\"");
        if (options.useChildPopOut && this.childPopOut) {
            if (this.isMap || this.mapChildren.Count > 0)
                builder.Append(this.mapChildren.Count > 0 ? "{^}" : "{}");
            if (this.isList || this.listChildren.Count > 0)
                builder.Append(this.listChildren.Count > 0 ? "[^]" : "[]");
        }
        else {
            if (this.isMap || this.mapChildren.Count > 0) {
                builder.Append("{");
                for (var i = 0, pair = null, pairs = this.mapChildren.Pairs; i < pairs.length && (pair = pairs[i]); i++) {
                    var keyStr = pair.key.ToVDF_InlinePart(options, tabDepth, true);
                    var valueStr = pair.value.ToVDF_InlinePart(options, tabDepth);
                    builder.Append((i == 0 ? "" : (options.useCommaSeparators ? "," : " ")) + (options.useStringKeys ? "\"" : "") + keyStr + (options.useStringKeys ? "\"" : "") + ":" + valueStr);
                }
                builder.Append("}");
            }
            if (this.isList || this.listChildren.Count > 0) {
                builder.Append("[");
                for (var i = 0; i < this.listChildren.Count; i++)
                    builder.Append((i == 0 ? "" : (options.useCommaSeparators ? "," : " ")) + this.listChildren[i].ToVDF_InlinePart(options, tabDepth));
                builder.Append("]");
            }
        }
        return builder.ToString();
    };
    VDFNode.prototype.ToVDF_PoppedOutPart = function (options, tabDepth) {
        if (options === void 0) { options = null; }
        if (tabDepth === void 0) { tabDepth = 0; }
        options = options || new VDFSaveOptions();
        var builder = new StringBuilder();
        // include popped-out-content of direct children (i.e. a single directly-under group)
        if (options.useChildPopOut && this.childPopOut) {
            var childTabStr = "";
            for (var i = 0; i < tabDepth + 1; i++)
                childTabStr += "\t";
            if (this.isMap || this.mapChildren.Count > 0)
                for (var i = 0, pair = null, pairs = this.mapChildren.Pairs; i < pairs.length && (pair = pairs[i]); i++) {
                    var keyStr = pair.key.ToVDF_InlinePart(options, tabDepth, true);
                    var valueStr = pair.value.ToVDF_InlinePart(options, tabDepth + 1);
                    builder.Append("\n" + childTabStr + (options.useStringKeys ? "\"" : "") + keyStr + (options.useStringKeys ? "\"" : "") + ":" + valueStr);
                    var poppedOutChildText = pair.value.ToVDF_PoppedOutPart(options, tabDepth + 1);
                    if (poppedOutChildText.length > 0)
                        builder.Append(poppedOutChildText);
                }
            if (this.isList || this.listChildren.Count > 0)
                for (var i in this.listChildren.Indexes()) {
                    var item = this.listChildren[i];
                    builder.Append("\n" + childTabStr + item.ToVDF_InlinePart(options, tabDepth + 1));
                    var poppedOutChildText = item.ToVDF_PoppedOutPart(options, tabDepth + 1);
                    if (poppedOutChildText.length > 0)
                        builder.Append(poppedOutChildText);
                }
        }
        else {
            var poppedOutChildTexts = new List("string");
            var poppedOutChildText;
            if (this.isMap || this.mapChildren.Count > 0)
                for (var i = 0, pair = null, pairs = this.mapChildren.Pairs; i < pairs.length && (pair = pairs[i]); i++)
                    if ((poppedOutChildText = pair.value.ToVDF_PoppedOutPart(options, tabDepth)).length)
                        poppedOutChildTexts.Add(poppedOutChildText);
            if (this.isList || this.listChildren.Count > 0)
                for (var i in this.listChildren.Indexes())
                    if ((poppedOutChildText = this.listChildren[i].ToVDF_PoppedOutPart(options, tabDepth)).length)
                        poppedOutChildTexts.Add(poppedOutChildText);
            for (var i = 0; i < poppedOutChildTexts.Count; i++) {
                poppedOutChildText = poppedOutChildTexts[i];
                var insertPoint = 0;
                while (poppedOutChildText[insertPoint] == '\n' || poppedOutChildText[insertPoint] == '\t')
                    insertPoint++;
                builder.Append((insertPoint > 0 ? poppedOutChildText.substr(0, insertPoint) : "") + (i == 0 ? "" : "^") + poppedOutChildText.substr(insertPoint));
            }
        }
        return builder.ToString();
    };
    // loading
    // ==================
    VDFNode.CreateNewInstanceOfType = function (typeName) {
        var typeNameRoot = VDF.GetTypeNameRoot(typeName);
        var genericParameters = VDF.GetGenericArgumentsOfType(typeName);
        /*if (typeNameRoot == "List")
            return new List(genericParameters[0]);
        if (typeNameRoot == "Dictionary")
            return new Dictionary(genericParameters[0], genericParameters[1]);*/
        if (typeName.Contains("("))
            //return window[typeNameRoot].apply(null, genericParameters);
            return new (Function.prototype.bind.apply(window[typeNameRoot], [null].concat(genericParameters)));
        if (!(window[typeNameRoot] instanceof Function))
            throw new Error("Could not find type \"" + typeName + "\".");
        return new window[typeNameRoot]; // maybe todo: add code that resets props to their nulled-out/zeroed-out values (or just don't use any constructors, and just remember to set the __proto__ property afterward)
    };
    VDFNode.GetCompatibleTypeNameForNode = function (node) { return node.mapChildren.Count ? "object" : (node.listChildren.length ? "List(object)" : "string"); };
    VDFNode.prototype.ToObject = function (declaredTypeName_orOptions, options, path) {
        if (options === void 0) { options = new VDFLoadOptions(); }
        if (declaredTypeName_orOptions instanceof VDFLoadOptions)
            return this.ToObject(null, declaredTypeName_orOptions);
        var declaredTypeName = declaredTypeName_orOptions;
        path = path || new VDFNodePath(new VDFNodePathNode());
        var fromVDFTypeName = "object";
        if (this.metadata != null && (window[VDF.GetTypeNameRoot(this.metadata)] instanceof Function || !options.loadUnknownTypesAsBasicTypes))
            fromVDFTypeName = this.metadata;
        else if (typeof this.primitiveValue == "boolean")
            fromVDFTypeName = "bool";
        else if (typeof this.primitiveValue == "number")
            fromVDFTypeName = this.primitiveValue.toString().Contains(".") ? "double" : "int";
        else if (typeof this.primitiveValue == "string")
            fromVDFTypeName = "string";
        else if (this.primitiveValue == null)
            if (this.isList || this.listChildren.Count > 0)
                fromVDFTypeName = "List(object)"; //"array";
            else if (this.isMap || this.mapChildren.Count > 0)
                fromVDFTypeName = "Dictionary(object object)"; //"object-anonymous"; //"object";
        var finalTypeName;
        if (window[VDF.GetTypeNameRoot(declaredTypeName)] instanceof Function || !options.loadUnknownTypesAsBasicTypes)
            finalTypeName = declaredTypeName;
        // if there is no declared type, or the from-metadata type is more specific than the declared type
        // (for last condition/way: also assume from-vdf-type is derived, if declared-type name is one of these extra (not actually implemented in JS) types)
        //if (finalTypeName == null || (<Function><object>window[VDF.GetTypeNameRoot(fromVDFTypeName)] || (()=>{})).IsDerivedFrom(<Function><object>window[VDF.GetTypeNameRoot(finalTypeName)] || (()=>{})) || ["object", "IList", "IDictionary"].Contains(finalTypeName))
        if (finalTypeName == null || VDF.IsTypeXDerivedFromY(fromVDFTypeName, finalTypeName) || ["object", "IList", "IDictionary"].Contains(finalTypeName))
            finalTypeName = fromVDFTypeName;
        var result;
        var deserializedByCustomMethod = false;
        var classProps = VDF.GetClassProps(window[finalTypeName]);
        for (var propName in classProps)
            if (classProps[propName] instanceof Function && classProps[propName].tags && classProps[propName].tags.Any(function (a) { return a instanceof VDFDeserialize && a.fromParent; })) {
                var deserializeResult = classProps[propName](this, path, options);
                if (deserializeResult != VDF.NoActionTaken) {
                    result = deserializeResult;
                    deserializedByCustomMethod = true;
                }
            }
        if (!deserializedByCustomMethod)
            if (finalTypeName == "object") { } //result = null;
            else if (EnumValue.IsEnum(finalTypeName))
                result = EnumValue.GetEnumIntForStringValue(finalTypeName, this.primitiveValue);
            else if (VDF.GetIsTypePrimitive(finalTypeName)) {
                result = this.primitiveValue;
                if (finalTypeName == "int")
                    result = parseInt(this.primitiveValue);
                else if (finalTypeName == "float" || finalTypeName == "double")
                    result = parseFloat(this.primitiveValue);
            }
            else if (this.primitiveValue != null || this.isList || this.isMap) {
                result = VDFNode.CreateNewInstanceOfType(finalTypeName);
                path.currentNode.obj = result;
                this.IntoObject(result, options, path);
            }
        path.currentNode.obj = result; // in case post-deserialize method was attached as extra-method to the object, that makes use of the (basically useless) path.currentNode.obj property
        return result;
    };
    VDFNode.prototype.IntoObject = function (obj, options, path) {
        if (options === void 0) { options = null; }
        options = options || new VDFLoadOptions();
        path = path || new VDFNodePath(new VDFNodePathNode(obj));
        var typeName = VDF.GetTypeNameOfObject(obj);
        var typeGenericArgs = VDF.GetGenericArgumentsOfType(typeName);
        var typeInfo = VDFTypeInfo.Get(typeName);
        for (var propName in VDF.GetObjectProps(obj))
            if (obj[propName] instanceof Function && obj[propName].tags && obj[propName].tags.Any(function (a) { return a instanceof VDFPreDeserialize; }))
                obj[propName](this, path, options);
        var deserializedByCustomMethod2 = false;
        for (var propName in VDF.GetObjectProps(obj))
            if (obj[propName] instanceof Function && obj[propName].tags && obj[propName].tags.Any(function (a) { return a instanceof VDFDeserialize && !a.fromParent; })) {
                var deserializeResult = obj[propName](this, path, options);
                if (deserializeResult != VDF.NoActionTaken)
                    deserializedByCustomMethod2 = true;
            }
        if (!deserializedByCustomMethod2) {
            for (var i = 0; i < this.listChildren.Count; i++) {
                //obj.Add(this.listChildren[i].ToObject(typeGenericArgs[0], options, path.ExtendAsListItem(i, this.listChildren[i])));
                var item = this.listChildren[i].ToObject(typeGenericArgs[0], options, path.ExtendAsListItem(i, this.listChildren[i]));
                if (obj.Count == i)
                    obj.Add(item);
            }
            for (var i = 0, pair = null, pairs = this.mapChildren.Pairs; i < pairs.length && (pair = pairs[i]); i++)
                try {
                    if (obj instanceof Dictionary) {
                        /*var key = VDF.Deserialize("\"" + keyString + "\"", typeGenericArgs[0], options);
                        //obj.Add(key, this.mapChildren[keyString].ToObject(typeGenericArgs[1], options, path.ExtendAsMapItem(key, null)));*/
                        var key = pair.key.ToObject(typeGenericArgs[0], options, path.ExtendAsMapKey(i, null));
                        var value = pair.value.ToObject(typeGenericArgs[1], options, path.ExtendAsMapItem(key, null));
                        obj.Set(key, value); // "obj" prop to be filled in at end of ToObject method // maybe temp; allow child to have already attached itself (by way of the VDF event methods)
                    }
                    else {
                        //obj[keyString] = this.mapChildren[keyString].ToObject(typeInfo.props[keyString] && typeInfo.props[keyString].typeName, options, path.ExtendAsChild(typeInfo.props[keyString] || { name: keyString }, null));
                        var propName = pair.key.primitiveValue;
                        /*if (typeInfo.props[propName]) // maybe temp; just ignore props that are missing
                        {*/
                        var value = pair.value.ToObject(typeInfo.props[propName] && typeInfo.props[propName].typeName, options, path.ExtendAsChild(typeInfo.props[propName] || { name: propName }, null));
                        obj[propName] = value;
                    }
                }
                /*catch (ex) {
                    ex.message += "\n==================\nRethrownAs) " + ("Error loading map-child with key '" + (typeof pair.key.primitiveValue == "string" ? "'" + pair.key.primitiveValue + "'" : "of type " + pair.key) + "'.") + "\n";
                    throw ex;
                } /**/
                finally { }
        }
        if (options.objPostDeserializeFuncs_early.ContainsKey(obj))
            for (var i in options.objPostDeserializeFuncs_early.Get(obj))
                options.objPostDeserializeFuncs_early.Get(obj)[i]();
        for (var propName in VDF.GetObjectProps(obj))
            if (obj[propName] instanceof Function && obj[propName].tags && obj[propName].tags.Any(function (a) { return a instanceof VDFPostDeserialize; }))
                obj[propName](this, path, options);
        if (options.objPostDeserializeFuncs.ContainsKey(obj))
            for (var i in options.objPostDeserializeFuncs.Get(obj))
                options.objPostDeserializeFuncs.Get(obj)[i]();
    };
    /*static charsThatNeedEscaping_1 = ['"', '\'', '\n'];
    static charsThatNeedEscaping_2 = ['{', '}', '[', ']', ':'];*/
    VDFNode.charsThatNeedEscaping_1_regex = /"|'|\n|<<|>>/;
    VDFNode.charsThatNeedEscaping_2_regex = /{|}|\[|\]|:/;
    return VDFNode;
})();
//VDFUtils.MakePropertiesHidden(VDFNode.prototype, true);
VDF.NoActionTaken = new VDFNode();
VDF.CancelSerialize = new VDFNode();
//# sourceMappingURL=VDFNode.js.map