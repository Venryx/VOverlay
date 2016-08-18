var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var VDFType = (function () {
    function VDFType(propIncludeRegexL1, popOutL1) {
        this.propIncludeRegexL1 = propIncludeRegexL1;
        this.popOutL1 = popOutL1;
    }
    VDFType.prototype.AddDataOf = function (typeTag) {
        if (typeTag.propIncludeRegexL1 != null)
            this.propIncludeRegexL1 = typeTag.propIncludeRegexL1;
        if (typeTag.popOutL1 != null)
            this.popOutL1 = typeTag.popOutL1;
    };
    return VDFType;
})();
var VDFTypeInfo = (function () {
    function VDFTypeInfo() {
        this.props = {};
    }
    VDFTypeInfo.Get = function (type_orTypeName) {
        //var type = type_orTypeName instanceof Function ? type_orTypeName : window[type_orTypeName];
        var typeName = type_orTypeName instanceof Function ? type_orTypeName.name : type_orTypeName;
        var typeNameBase = typeName.Contains("(") ? typeName.substr(0, typeName.indexOf("(")) : typeName;
        if (VDF.GetIsTypeAnonymous(typeNameBase)) {
            var result = new VDFTypeInfo();
            result.typeTag = new VDFType(VDF.PropRegex_Any);
            return result;
        }
        var typeBase = window[typeNameBase];
        if (typeBase && typeBase.typeInfo == null) {
            var result = new VDFTypeInfo();
            result.typeTag = new VDFType();
            /*var typeTag = new VDFType();
            var currentTypeName = typeNameBase;
            while (window[currentTypeName]) //true)
            {
                if (window[currentTypeName].typeInfo.typeTag)
                    for (var key in window[currentTypeName].typeInfo.typeTag)
                        if (typeTag[key] == null)
                            typeTag[key] = window[currentTypeName].typeInfo.typeTag[key];

                if (window[currentTypeName].prototype && window[currentTypeName].prototype.__proto__) // if has base-type
                    currentTypeName = window[currentTypeName].prototype.__proto__.constructor.name; // set current-type-name to base-type's name
                else
                    break;
            }
            result.typeTag = typeTag;*/
            var currentType = typeNameBase;
            while (currentType != null) {
                var currentTypeConstructor = window[currentType];
                var typeTag2 = (currentTypeConstructor.typeInfo || {}).typeTag;
                for (var key in typeTag2)
                    if (result.typeTag[key] == null)
                        result.typeTag[key] = typeTag2[key];
                currentType = currentTypeConstructor.prototype && currentTypeConstructor.prototype.__proto__ && currentTypeConstructor.prototype.__proto__.constructor.name;
            }
            typeBase.typeInfo = result;
        }
        return typeBase && typeBase.typeInfo;
    };
    VDFTypeInfo.prototype.GetProp = function (propName) {
        if (!(propName in this.props))
            this.props[propName] = new VDFPropInfo(propName, null, [], null, null);
        return this.props[propName];
    };
    return VDFTypeInfo;
})();
var VDFProp = (function () {
    function VDFProp(includeL2, popOutL2) {
        if (includeL2 === void 0) { includeL2 = true; }
        this.includeL2 = includeL2;
        this.popOutL2 = popOutL2;
    }
    return VDFProp;
})();
var P = (function (_super) {
    __extends(P, _super);
    function P(includeL2, popOutL2) {
        if (includeL2 === void 0) { includeL2 = true; }
        _super.call(this, includeL2, popOutL2);
    }
    return P;
})(VDFProp);
var DefaultValue = (function () {
    function DefaultValue(defaultValue) {
        if (defaultValue === void 0) { defaultValue = D.DefaultDefault; }
        this.defaultValue = defaultValue;
    }
    return DefaultValue;
})();
var D = (function (_super) {
    __extends(D, _super);
    function D(defaultValue) {
        if (defaultValue === void 0) { defaultValue = D.DefaultDefault; }
        _super.call(this, defaultValue);
    }
    //static NoDefault = new object(); // i.e. the prop has no default, so whatever value it has is always saved [commented out, since: if you want no default, just don't add the D tag]
    D.DefaultDefault = new object(); // i.e. the default value for the type (not the prop) ['false' for a bool, etc.]
    D.NullOrEmpty = new object(); // i.e. null, or an empty string or collection
    D.Empty = new object(); // i.e. an empty string or collection
    return D;
})(DefaultValue);
var VDFPropInfo = (function () {
    function VDFPropInfo(propName, propTypeName, tags, propTag, defaultValueTag) {
        this.name = propName;
        this.typeName = propTypeName;
        this.tags = tags;
        this.propTag = propTag;
        this.defaultValueTag = defaultValueTag;
    }
    VDFPropInfo.prototype.ShouldValueBeSaved = function (val) {
        //if (this.defaultValueTag == null || this.defaultValueTag.defaultValue == D.NoDefault)
        if (this.defaultValueTag == null)
            return true;
        if (this.defaultValueTag.defaultValue == D.DefaultDefault) {
            if (val == null)
                return false;
            if (val === false || val === 0)
                return true;
        }
        if (this.defaultValueTag.defaultValue == D.NullOrEmpty && val === null)
            return false;
        if (this.defaultValueTag.defaultValue == D.NullOrEmpty || this.defaultValueTag.defaultValue == D.Empty) {
            var typeName = VDF.GetTypeNameOfObject(val);
            if (typeName && typeName.startsWith("List(") && val.length == 0)
                return false;
            if (typeName == "string" && !val.length)
                return false;
        }
        if (val === this.defaultValueTag.defaultValue)
            return false;
        return true;
    };
    return VDFPropInfo;
})();
var VDFPreSerializeProp = (function () {
    function VDFPreSerializeProp() {
    }
    return VDFPreSerializeProp;
})();
var VDFPreSerialize = (function () {
    function VDFPreSerialize() {
    }
    return VDFPreSerialize;
})();
var VDFSerialize = (function () {
    function VDFSerialize() {
    }
    return VDFSerialize;
})();
var VDFPostSerialize = (function () {
    function VDFPostSerialize() {
    }
    return VDFPostSerialize;
})();
var VDFPreDeserialize = (function () {
    function VDFPreDeserialize() {
    }
    return VDFPreDeserialize;
})();
var VDFDeserialize = (function () {
    function VDFDeserialize(fromParent) {
        if (fromParent === void 0) { fromParent = false; }
        this.fromParent = fromParent;
    }
    return VDFDeserialize;
})();
var VDFPostDeserialize = (function () {
    function VDFPostDeserialize() {
    }
    return VDFPostDeserialize;
})();
/*class VDFMethodInfo
{
    tags: any[];
    constructor(tags: any[]) { this.tags = tags; }
}*/ 
//# sourceMappingURL=VDFTypeInfo.js.map