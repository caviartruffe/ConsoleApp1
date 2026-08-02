using manage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace manage
{
    public class InfoDocument
    {
        [Flags]
        public enum FolderState
        {
            None = 0,
            FileUploadDone = 1,
            FileConvertDone = 2,
            NumberingRequest = 4,
            NumberingDone = 8,
            RelationRequest = 16,
            RelationDone = 32,
            RegistrationRequest = 64,
            RegistrationDone = 128,
        }

        public static readonly Dictionary<FolderState, string> _folderStateFiles = new()
        {
            [FolderState.FileUploadDone] = ".upload_file.done",
            [FolderState.FileConvertDone] = ".convert_file.done",
            [FolderState.NumberingRequest] = ".numbering.request",
            [FolderState.NumberingDone] = ".numbering.done",
            [FolderState.RelationRequest] = ".relation.request",
            [FolderState.RelationDone] = ".relation.done",
            [FolderState.RegistrationRequest] = ".registration.request",
            [FolderState.RegistrationDone] = ".registration.done"
        };

        /// <summary>
        /// フォルダ処理進行状態を取得
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public FolderState GetFolderState()
        {
            var folderState = FolderState.None;

            foreach (FolderState state in Enum.GetValues(typeof(FolderState)))
            {
                if (File.Exists(Path.Combine(FolderPath, _folderStateFiles[state])))
                {
                    folderState |= state;
                }
            }
            return folderState;
        }

        public void CreateFolderState(FolderState state)
        {
            // Dictionaryから拡張子（ファイル名）を取得
            if (_folderStateFiles.TryGetValue(state, out string? fileName))
            {
                string fullPath = Path.Combine(FolderPath, fileName);
                // サイズ0のフラグファイルを生成
                using (File.Create(fullPath)) { }
            }
        }

        public bool IsNumberAssigned { get => string.IsNullOrEmpty(Number); }
        public string FolderPath { get; set; }

        // ドキュメントID
        public int RegId { get; set; } 
        public string Number { get; set; } = string.Empty;
        public List<InfoUploadFile> UploadFileInfos { get; set; } = new List<InfoUploadFile>();

        public InfoDocument()
        {
            // 
        }

        public void SetSftpFunction(SftpAccess.SftpFunction func)
        {
            var basename = SftpAccess.GetSftpBaseName(func, RegId);
            //SftpControlFileName = basename + ".tsv";
            //SftpResultFileName = basename + ".tsv.done";
            //SftpListFileName = func == SftpAccess.SftpFunction.Registration ?
            //    basename + ".lst" : string.Empty;
        }
    }
}
