using KamiToolKit.System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CurrencyAlert.Classes;

public static class NodeBaseExtensions {
	public static void Load(this NodeBase caller, NodeBase other) {
		var serializedOther = JsonConvert.SerializeObject(other, Formatting.Indented);
		var reserialized = (JObject?) JsonConvert.DeserializeObject(serializedOther);
		if (reserialized is not null) {
			reserialized.Remove("Position");
			reserialized.Remove("Size");
			JsonConvert.PopulateObject(reserialized.ToString(), caller);
		}
	}
}