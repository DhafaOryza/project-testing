// using UnityEngine;

// namespace TrafficCrossing.CoreGame.Obstacle
// {
//     public class TerrainChunk : MonoBehaviour
//     {
//         [Header("Grid Row Objects")]
//         [Tooltip("Masukkan child GameObject per-baris grid sesuai urutan index (Index 0 = Row 0, dst.)")]
//         [SerializeField] private GameObject[] _gridRows;

//         // private void Onable()
//         // {
//         //     ResetAllRows();           
//         // }

//         /// <summary>
//         /// Menonaktifkan visual/collider pada baris grid tertentu.
//         /// </summary>
//         public void HideRow(int gridOffset)
//         {
//             if (_gridRows != null && gridOffset >= 0 && gridOffset < _gridRows.Length)
//             {
//                 if (_gridRows[gridOffset] != null)
//                 {
//                     _gridRows[gridOffset].SetActive(false);
//                 }
//             }
//         }

//         /// <summary>
//         /// Mengembalikan semua baris ke kondisi aktif saat chunk di-recycle/reuse dari Pool.
//         /// </summary>
//         public void ResetAllRows()
//         {
//             if (_gridRows == null) return;

//             foreach (GameObject row in _gridRows)
//             {
//                 if (row != null)
//                 {
//                     row.SetActive(true);
//                 }
//             }
//         }
//     }
// }