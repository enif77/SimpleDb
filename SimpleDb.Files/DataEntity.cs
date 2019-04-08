/* SimpleDb - (C) 2016 - 2019 Premysl Fara 
 
SimpleDb is available under the zlib license:

This software is provided 'as-is', without any express or implied
warranty.  In no event will the authors be held liable for any damages
arising from the use of this software.

Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:

1. The origin of this software must not be misrepresented; you must not
   claim that you wrote the original software. If you use this software
   in a product, an acknowledgment in the product documentation would be
   appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be
   misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
 
 */

namespace SimpleDb.Files
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    

    /// <summary>
    /// Represents data, that can be stored in a file.
    /// </summary>
    public class DataEntity
    {
        /// <summary>
        /// The maximum column name length.
        /// </summary>
        public static readonly int MaxColumnNameLength = 256;

        /// <summary>
        /// The NULL representation in a string.
        /// </summary>
        private static readonly string NullStringRepresentation = "\x1b\x4e";

        /// <summary>
        /// The LF ("\n") representation in a string.
        /// </summary>
        private static readonly string NewLineStringRepresentation = "\x1b\x4c";
        

        /// <summary>
        /// The parsed values dictionary.
        /// </summary>
        public Dictionary<string, string> Values { get; }


        /// <summary>
        /// Constructor.
        /// </summary>
        public DataEntity()
        {
            Values = new Dictionary<string, string>();
        }


        /// <summary>
        /// Inserts a new value. 
        /// Does not allow to insert a duplicity key.
        /// A null value is never inserted into the Values dictionary.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        public void InsertValue(string key, string value)
        {
            IsValidKey(key);

            if (Values.ContainsKey(key))
            {
                throw new InvalidOperationException(string.Format("The duplicate key '{0}' can not be inserted.", key));
            }

            if (value == null) return;

            Values.Add(key, value);
        }

        /// <summary>
        /// Inserts a new value. 
        /// Allows to set a new value to an existing key. 
        /// A null value is never inserted into the Values dictionary and removes an existing key/value pair.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        public void SetValue(string key, string value)
        {
            IsValidKey(key);

            if (Values.ContainsKey(key))
            {
                if (value == null)
                {
                    Values.Remove(key);
                }
                else
                {
                    Values[key] = value;
                }
            }
            else
            {
                if (value == null) return;

                Values.Add(key, value);
            }
        }

        /// <summary>
        /// Returns a value of a certain key or null if no such key/value pair exists.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <returns>A value or null.</returns>
        public string GetValue(string key)
        {
            IsValidKey(key);

            return Values.ContainsKey(key) ? Values[key] : null;
        }

        /// <summary>
        /// Returns a value of a certain key or false if no such key/value pair exists.
        /// A value is considered to be true, when it equals to the "true" string only.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <returns>True or false.</returns>
        public bool GetBoolValue(string key)
        {
            IsValidKey(key);

            return Values.ContainsKey(key) && (Values[key] == "true");
        }

        /// <summary>
        /// Inserts a new boolean value.
        /// A boolean value is stored as a "true" or "false" string.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="v">A value.</param>
        public void SetValue(string key, bool v)
        {
            SetValue(key, v ? "true" : "false");
        }

        /// <summary>
        /// Returns a value of a certain key or DateTime.MinValue if no such key/value pair exists.
        /// A value is expected to be in the "yyyyMMddHHmmss" format.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <returns>True or false.</returns>
        public DateTime GetDateTimeValue(string key)
        {
            IsValidKey(key);

            return Values.ContainsKey(key)
                ? DateTime.ParseExact(Values[key], "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces)
                : DateTime.MinValue;
        }

        /// <summary>
        /// Inserts a new date-time value.
        /// A DateTime value is stored as a "yyyyMMddHHmmss" formated string.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="v">A value.</param>
        public void SetValue(string key, DateTime v)
        {
            SetValue(key, v.ToString("yyyyMMddHHmmss"));
        }

        /// <summary>
        /// Returns a value of a certain key or 0 if no such key/value pair exists.
        /// A value is expected to be in the NumberStyles.Number, CultureInfo.InvariantCulture format.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <returns>True or false.</returns>
        public decimal GetDecimalValue(string key)
        {
            IsValidKey(key);

            return Values.ContainsKey(key)
                ? Decimal.Parse(Values[key], NumberStyles.Number, CultureInfo.InvariantCulture)
                : 0;
        }

        /// <summary>
        /// Inserts a new decimal value.
        /// A DateTime value is stored as a CultureInfo.InvariantCulture formated string.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="v">A value.</param>
        public void SetValue(string key, decimal v)
        {
            SetValue(key, v.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Returns a value of a certain key or 0 if no such key/value pair exists.
        /// A value is expected to be in the NumberStyles.Integer, CultureInfo.InvariantCulture format.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <returns>True or false.</returns>
        public int GetInt32Value(string key)
        {
            IsValidKey(key);

            return Values.ContainsKey(key) 
                ? Int32.Parse(Values[key], NumberStyles.Integer, CultureInfo.InvariantCulture) 
                : 0;
        }

        /// <summary>
        /// Inserts a new int value.
        /// A DateTime value is stored as a CultureInfo.InvariantCulture formated string.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="v">A value.</param>
        public void SetValue(string key, int v)
        {
            SetValue(key, v.ToString(CultureInfo.InvariantCulture));
        }


        /// <summary>
        /// Loads an entity froma file.
        /// </summary>
        /// <param name="fileName">A file containing the entity.</param>
        /// <returns>A loaded entity or null.</returns>
        public static DataEntity LoadDataEntity(string fileName)
        {
            if (String.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("A file name expected.", "fileName");
            }

            var data = FileHelper.LoadDataAsArrayOfStrings(fileName);
            var dataEntity = new DataEntity();

            for (var line = 0; line < data.Length; line++)
            {
                var dataLine = data[line];

                // Ignore empty lines.
                if (String.IsNullOrWhiteSpace(dataLine)) continue;

                // Is it a valid data line format?
                var columnSeparatorIndex = dataLine.IndexOf(':');
                if (columnSeparatorIndex < 0 || columnSeparatorIndex >= MaxColumnNameLength)
                {
                    throw new ApplicationException(String.Format("Bad column data format at line {0}, in file {1}.", line, fileName));
                }

                // Get and check the column name.
                try
                {
                    dataEntity.InsertValue(
                        dataLine.Substring(0, columnSeparatorIndex).Trim(),
                        DecodeStringData(dataLine.Substring(columnSeparatorIndex + 1)));
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(String.Format("Can not insert column data at line {0}, in file {1}.", line, fileName), ex);
                }
            }

            return dataEntity;
        }

        /// <summary>
        /// Saves an data entity into a file.
        /// </summary>
        /// <param name="entity">An entity.</param>
        /// <param name="fileName">An output file name.</param>
        public static void SaveDataEntity(DataEntity entity, string fileName)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            if (String.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("A file name expected.", "fileName");
            }

            using (var file = new StreamWriter(fileName))
            {
                foreach (string columnName in entity.Values.Keys)
                {
                    var line = String.Format("{0}:{1}", columnName, EncodeStringData(entity.Values[columnName]));

                    file.WriteLine(line);
                }
            }
        }

        /// <summary>
        /// Checks, if a key is a valid database key.
        /// </summary>
        /// <param name="key"></param>
        private static void IsValidKey(string key)
        {
            if (String.IsNullOrWhiteSpace(key))
            {
                throw new ApplicationException("The column name can not be empty");
            }
        }

        /// <summary>
        /// Decodes string data.
        /// </summary>
        /// <param name="data">A raw string data.</param>
        /// <returns>Decoded string data.</returns>
        private static string DecodeStringData(string data)
        {
            if (String.IsNullOrWhiteSpace(data))
            {
                // Nothing to do here.
                return data;
            }

            if (data == NullStringRepresentation)
            {
                // It is a NULL.
                return null;
            }

            // It is a string, replace the LF encoding with the LF itself.
            return data.Replace(NewLineStringRepresentation, "\n");
        }

        /// <summary>
        /// Encodes string data into the in-file encodding.
        /// </summary>
        /// <param name="data">String data.</param>
        /// <returns>Encoded string data.</returns>
        private static string EncodeStringData(string data)
        {
            if (data == null)
            {
                return NullStringRepresentation;
            }

            return data.Replace("\n", NewLineStringRepresentation);
        }
    }
}
