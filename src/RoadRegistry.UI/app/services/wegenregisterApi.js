import axios from 'axios';
import fileSaver from 'file-saver';

function updateDownloadRegistryProgress(lastUpdate, loaded) {
  const KB = Math.round(loaded / 1024);
  const MB = Math.round(KB / 102.4) / 10;
  const minimumProgress = 0.14;

  if (MB < 1 || MB > lastUpdate + minimumProgress) {
    console.log(`Downloaded: ${MB} MB`);
    return MB;
  }
  return lastUpdate;
}

export default {
  downloadCompleteRegistry() {
    let progress = 0;

    return axios
      .get(
        '/v1/extracten',
        {
          transformRequest: [(data, headers) => {
            // eslint-disable-next-line
            delete headers.common.Accept;
            return data;
          }],
          onDownloadProgress: ({ loaded }) => {
            progress = updateDownloadRegistryProgress(progress, loaded);
          },

          headers: {
            // 'Access-Control-Expose-Headers': 'Content-Disposition',
            'Content-Type': 'application/zip',
          },
          responseType: 'blob',
        },
      )
      .then((response) => {
        // if I get the CORS settings correct, the filename can be retrieved from response.Headers['Content-Disposition']
        fileSaver.saveAs(response.data, 'wegenregister.zip');
      });
  },
};
