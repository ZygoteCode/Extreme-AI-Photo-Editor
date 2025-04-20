using Emgu.CV;
using Emgu.CV.Structure;

public class AiImageProcessing
{
    public static System.Drawing.Image BlurAllFaces(System.Drawing.Image image, int frontalModel, int profileModel, int kernelSize)
    {
        Image<Bgr, byte> img = ((System.Drawing.Bitmap)image).ToImage<Bgr, byte>();
        Image<Gray, byte> grayImg = img.Convert<Gray, byte>();

        {
            CascadeClassifier faceCascade = new CascadeClassifier($"models\\frontal_face_recognition\\model_{frontalModel + 1}.xml");
            System.Drawing.Rectangle[] faces = faceCascade.DetectMultiScale(grayImg, 1.1, 3, System.Drawing.Size.Empty);

            foreach (System.Drawing.Rectangle face in faces)
            {
                Image<Bgr, byte> subRectangle = img.GetSubRect(face);
                CvInvoke.GaussianBlur(subRectangle, subRectangle, new System.Drawing.Size(kernelSize, kernelSize), 0);
            }
        }

        {
            CascadeClassifier faceCascade = new CascadeClassifier($"models\\profile_face_recognition\\model_{profileModel + 1}.xml");
            System.Drawing.Rectangle[] faces = faceCascade.DetectMultiScale(grayImg, 1.1, 3, System.Drawing.Size.Empty);

            foreach (System.Drawing.Rectangle face in faces)
            {
                Image<Bgr, byte> subRectangle = img.GetSubRect(face);
                CvInvoke.GaussianBlur(subRectangle, subRectangle, new System.Drawing.Size(kernelSize, kernelSize), 0);
            }
        }

        return img.ToBitmap();
    }
}