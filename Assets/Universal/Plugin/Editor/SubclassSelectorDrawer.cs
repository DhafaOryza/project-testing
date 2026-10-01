namespace Assets.PolyRef
{    
    using System;
    using System.Linq;
    using Assets.PolyRef;
    using UnityEditor;
    using UnityEngine;

    // Memberitahu Unity: "Gunakan script ini untuk menggambar variabel yang punya stiker SubclassSelector"
    [CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
    public class SubclassSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Mencegah error jika [SerializeReference] lupa dipasang 
            // atau jika Unity salah mengirim tipe data ke Drawer ini.
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUI.LabelField(position, label.text, "Error: Butuh [SerializeReference]!");
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            // 1. Dapatkan tipe dasar (Base Type) dari variabel tersebut
            Type baseType = fieldInfo.FieldType;
            
            // Cek jika variabelnya berupa List atau Array, kita ambil tipe elemen di dalamnya
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
                baseType = baseType.GetGenericArguments()[0];
            else if (baseType.IsArray)
                baseType = baseType.GetElementType();

            // 2. Buat kotak untuk tombol Dropdown
            Rect dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            dropdownRect = EditorGUI.PrefixLabel(dropdownRect, label);

            // 3. Cek tipe class apa yang sedang aktif saat ini
            string currentTypeName = property.managedReferenceValue != null 
                ? property.managedReferenceValue.GetType().Name 
                : "Null (Klik untuk memilih)";

            // 4. Gambar tombol Dropdown. Jika diklik, tampilkan menu
            if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(currentTypeName), FocusType.Keyboard))
            {
                ShowDropdown(property, baseType);
            }

            // 5. Gambar variabel-variabel di dalam class yang dipilih (jika tidak Null)
            if (property.managedReferenceValue != null)
            {
                Rect propertyRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, position.height - EditorGUIUtility.singleLineHeight - 2);
                EditorGUI.PropertyField(propertyRect, property, GUIContent.none, true);
            }

            EditorGUI.EndProperty();
        }

        // Menentukan tinggi total dari UI di Inspector
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // GUARD: Jika bukan ManagedReference, kembalikan tinggi standar (1 baris)
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                return EditorGUIUtility.singleLineHeight;
            }
            
            if (property.managedReferenceValue != null)
            {
                // Tinggi tombol + Tinggi variabel di dalam class
                return EditorGUI.GetPropertyHeight(property, true) + EditorGUIUtility.singleLineHeight + 2;
            }
            // Hanya setinggi tombol jika isinya Null
            return EditorGUIUtility.singleLineHeight;
        }

        // Menampilkan menu pop-up (Dropdown)
        private void ShowDropdown(SerializedProperty property, Type baseType)
        {
            GenericMenu menu = new GenericMenu();
            
            // Opsi untuk mengosongkan data (Null)
            menu.AddItem(new GUIContent("Clear (Null)"), false, () => AssignNewInstance(property, null));
            menu.AddSeparator("");

            // Fitur ajaib Unity: TypeCache mencari SEMUA class turunan di project dengan sangat cepat
            var types = TypeCache.GetTypesDerivedFrom(baseType)
                .Where(t => !t.IsAbstract && !t.IsInterface); // Abaikan class abstract & interface

            foreach (Type type in types)
            {
                menu.AddItem(new GUIContent(type.Name), false, () => AssignNewInstance(property, type));
            }

            menu.ShowAsContext();
        }

        // Membuat objek baru (Instansiasi) saat pengguna memilih opsi di menu
        private void AssignNewInstance(SerializedProperty property, Type type)
        {
            // Activator.CreateInstance berfungsi seperti memanggil fungsi "new NamaClass()"
            object newInstance = type != null ? Activator.CreateInstance(type) : null;
            
            property.serializedObject.Update();
            property.managedReferenceValue = newInstance; // Simpan ke dalam SerializeReference
            property.serializedObject.ApplyModifiedProperties(); // Wajib agar bisa di-Undo/Redo
        }
    }
}
