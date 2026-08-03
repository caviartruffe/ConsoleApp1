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
        public enum FolderStates
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

        public static readonly Dictionary<FolderStates, string> _folderStateFiles = new()
        {
            [FolderStates.FileUploadDone] = ".upload_file.done",
            [FolderStates.FileConvertDone] = ".convert_file.done",
            [FolderStates.NumberingRequest] = ".numbering.request",
            [FolderStates.NumberingDone] = ".numbering.done",
            [FolderStates.RelationRequest] = ".relation.request",
            [FolderStates.RelationDone] = ".relation.done",
            [FolderStates.RegistrationRequest] = ".registration.request",
            [FolderStates.RegistrationDone] = ".registration.done"
        };

        /// <summary>
        /// フォルダ処理進行状態を取得
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public FolderStates GetFolderState()
        {
            var folderState = FolderStates.None;

            foreach (FolderStates state in Enum.GetValues(typeof(FolderStates)))
            {
                if (File.Exists(Path.Combine(FolderPath, _folderStateFiles[state])))
                {
                    folderState |= state;
                }
            }
            return folderState;
        }

        //---------------------------------------------

        public void SetFolderState(FolderStates state)
        {
            FolderState = state;
            CreateFolderState(state);
        }

        public void CreateFolderState(FolderStates state)
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

        public FolderStates FolderState { get; set; }

        // ドキュメントID
        public int DocRegId { get; set; } 
        public string Number { get; set; } = string.Empty;
        public List<InfoUploadFile> UploadFileInfos { get; set; } = new List<InfoUploadFile>();

        public InfoDocument()
        {
            // 
        }

        public void SetSftpFunction(SftpAccess.SftpFunction func)
        {
            var basename = SftpAccess.GetSftpBaseName(func, DocRegId);
            //SftpControlFileName = basename + ".tsv";
            //SftpResultFileName = basename + ".tsv.done";
            //SftpListFileName = func == SftpAccess.SftpFunction.Registration ?
            //    basename + ".lst" : string.Empty;
        }
    }
}
