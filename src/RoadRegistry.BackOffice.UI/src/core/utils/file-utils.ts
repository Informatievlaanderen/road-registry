const FileUtils = {
  downloadFile: (url: string, fileName: string) => {
    const link = document.createElement("a");
    link.href = url;
    link.download = fileName;
    link.click();
    link.remove();
  },
};

export const downloadFile = FileUtils.downloadFile;
export default FileUtils;
