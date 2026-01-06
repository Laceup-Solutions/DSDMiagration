namespace LaceupMigration
{
    public class KeyValuePairWritable<TKey, TValue>
    {
        //
        // Summary:
        //     Initializes a new instance of the System.Collections.Generic.KeyValuePair<TKey,TValue>
        //     structure with the specified key and value.
        //
        // Parameters:
        //   key:
        //     The object defined in each key/value pair.
        //
        //   value:
        //     The definition associated with key.
        public KeyValuePairWritable(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        // Summary:
        //     Gets the key in the key/value pair.
        //
        // Returns:
        //     A TKey that is the key of the System.Collections.Generic.KeyValuePair<TKey,TValue>.
        public TKey Key { get; set; }
        //
        // Summary:
        //     Gets the value in the key/value pair.
        //
        // Returns:
        //     A TValue that is the value of the System.Collections.Generic.KeyValuePair<TKey,TValue>.
        public TValue Value { get; set; }
    }
}