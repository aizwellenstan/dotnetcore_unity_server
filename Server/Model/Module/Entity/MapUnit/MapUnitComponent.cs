using System.Collections.Generic;
using System.Linq;
using ETHotfix;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace ETModel
{
    public class MapUnitComponent : Component
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private readonly Dictionary<long, MapUnit> idUnits = new Dictionary<long, MapUnit>();

        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private readonly Dictionary<long, MapUnit> uidUnits = new Dictionary<long, MapUnit>();

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            base.Dispose();

            foreach (MapUnit unit in this.idUnits.Values)
            {
                unit.Dispose();
            }
            this.idUnits.Clear();
            this.uidUnits.Clear();
        }

        public void Add(MapUnit unit)
        {
            if(this.uidUnits.TryAdd(unit.Uid, unit))
            {
                this.idUnits.Add(unit.Id, unit);
            }
        }

        public MapUnit Get(long id)
        {
            this.idUnits.TryGetValue(id, out MapUnit mapUnit);
            return mapUnit;
        }

        public MapUnit GetByUid(long uid)
        {
            this.uidUnits.TryGetValue(uid, out MapUnit mapUnit);
            return mapUnit;
        }

        public void Remove(long id)
        {
            if(this.idUnits.TryGetValue(id, out MapUnit mapUnit))
            {
                this.idUnits.Remove(id);
                this.uidUnits.Remove(mapUnit.Uid);
                mapUnit.Dispose();
            }
        }

        public void RemoveNoDispose(long id)
        {
            this.idUnits.Remove(id);
        }

        public int Count
        {
            get
            {
                return this.idUnits.Count;
            }
        }

        public MapUnit[] GetAll()
        {
            return this.idUnits.Values.ToArray();
        }
    }
}