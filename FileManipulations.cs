using MD5Hash;
using Serilog;

namespace FolderSyncer
{
    public record FileData(string LocationOldFolder, string LocationNewFolder, string MD5);

    public class FileManipulations
    {

        public List<FileData> FilesData { get; set; } = new();

        public void Cleanup(string newFolderPath, string initFolderPath, ILogger logger)
        {
            var allNewFilePaths = Directory.GetFiles(newFolderPath, "*.*", SearchOption.AllDirectories);
            var allOldFilePaths = Directory.GetFiles(initFolderPath, "*.*", SearchOption.AllDirectories);

            foreach (var filePath in allNewFilePaths)
            {
                if (!allOldFilePaths.Any(x => x == filePath.Replace(newFolderPath, initFolderPath)))
                {
                    TryDelete(filePath, logger);
                }
            }

            var allNewDirectories = Directory.GetDirectories(newFolderPath, "*", SearchOption.AllDirectories);
            var allOldDirectories = Directory.GetDirectories(initFolderPath, "*", SearchOption.AllDirectories);

            foreach (var directoryPath in allNewDirectories)
            {
                if (!allOldDirectories.Any(x => x == directoryPath.Replace(newFolderPath, initFolderPath)))
                {
                    if (Directory.Exists(directoryPath))
                    {
                        Directory.Delete(directoryPath, true);
                        logger.Information($"{directoryPath} is outdated and has to be deleted.");
                    }
                }
            }
        }

        public void Manipulate(string initFolderPath, string newFolderPath, ILogger logger)
        {
            var allDirectories = Directory.GetDirectories(initFolderPath, "*", SearchOption.AllDirectories);

            foreach (string dir in allDirectories)
            {
                var dirToCreate = dir.Replace(initFolderPath, newFolderPath);
                if (!Directory.Exists(dirToCreate))
                {
                    var directory = Directory.CreateDirectory(dirToCreate);
                    logger.Information($"{directory.Name} folder sucessfully created.");
                }
            }

            var allFilesInit = Directory.GetFiles(initFolderPath, "*.*", SearchOption.AllDirectories);

            foreach (string oldPath in allFilesInit)
            {
                var newPath = oldPath.Replace(initFolderPath, newFolderPath);

                //it might be that during long operations file will get deleted before we get here. In this case, we need to check for an existence before proceeding further.
                if (File.Exists(oldPath))
                {

                    var stream = File.OpenRead(oldPath);
                    var fileDataPiece = new FileData(oldPath, newPath, stream.GetMD5());
                    stream.Close();

                    //If file was not found previously.
                    if (!this.FilesData.Contains(fileDataPiece))
                    {

                        if (File.Exists(fileDataPiece.LocationNewFolder))
                        {
                            TryDelete(fileDataPiece.LocationNewFolder, logger);
         
                        }
                        
                        var fileDataFound = this.FilesData.FirstOrDefault(x => x!.LocationOldFolder == fileDataPiece.LocationOldFolder, null);

                        if (fileDataFound != null) this.FilesData.Remove(fileDataFound);

                        this.FilesData.Add(fileDataPiece);
                        TryCopy(oldPath, newPath, logger);
                    } 
                    else
                    {
                        if (File.Exists(fileDataPiece.LocationNewFolder))
                        {
                            var streamNewFile = File.OpenRead(fileDataPiece.LocationNewFolder);

                            if (streamNewFile.GetMD5() != fileDataPiece.MD5)
                            {
                                streamNewFile.Close();
                                TryDelete(fileDataPiece.LocationNewFolder, logger);
                                TryCopy(oldPath, newPath, logger);
                            }

                            streamNewFile.Close();
                        }
                        else
                        {
                            //No need for try and catch here since file had to be deleted from new folder if program got to this stage.
                            File.Copy(oldPath, newPath, true);
                            logger.Information($"{oldPath} sucessfully copied to {newPath}.");
                        }   
                    } 
                }
                else 
                {
                    logger.Information($"{oldPath} does not exist anymore.");
                } 
            }

            Cleanup(newFolderPath, initFolderPath, logger);
            logger.Information("Directory sucessfully copied.");
        }

        public void TryCopy(string oldPath, string newPath, ILogger logger)
        {
            try
            {
                File.Copy(oldPath, newPath, true);
                logger.Information($"{oldPath} sucessfully copied to {newPath}.");
            }
            catch (IOException)
            {
                logger.Information($"{newPath} could not be updated as it is being used by another process.");
            }
        }

        public void TryDelete(string path, ILogger logger)
        {
            try
            {
                File.Delete(path);
                logger.Information($"{path} was outdated and had to be deleted.");
            }
            catch (IOException)
            {
                logger.Information($"{path} could not be deleted as it is being used by another process.");
            }
        }
    }
}
