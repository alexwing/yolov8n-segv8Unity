# Object Detection with YOLOv8n-seg in Unity

## Introduction

The GitHub repository [yolov8n-segv8Unity](https://github.com/alexwing/yolov8n-segv8Unity) demonstrates the use of the YOLOv8n-seg object detection model in Unity, serving as a foundation for real-time object detection projects. This report details the integration of the YOLOv8n-seg model in Unity, the training process, its performance on mobile devices, and the benefits of segmentation for creating engaging augmented reality (AR) effects.

## Integration of YOLOv8n-seg in Unity

Integrating the YOLOv8n-seg model in Unity enables real-time processing of camera images, object detection, and result visualization in an AR interface. The main steps are:

### 1. Model Conversion

The YOLOv8n-seg model, originally in PyTorch format (`.pt`), must be converted to ONNX for compatibility with Unity Barracuda, Unity’s machine learning inference engine. This is achieved using the Ultralytics library with the following command:

```bash
yolo export model=yolov8n-seg.pt format=onnx imgsz=640 opset=9
```

The `opset=9` parameter ensures compatibility with Barracuda, as higher versions may cause errors with operations like `Split`. The resulting ONNX file includes outputs for detection and segmentation (bounding boxes and masks).

### 2. Importing the Model into Unity

The `yolov8n-seg.onnx` file is placed in the `Assets` folder of the Unity project. Unity 2022.3 or later is recommended, as it supports the necessary features for machine learning and AR. The project structure is based on the [YOLOv8Unity](https://github.com/wojciechp6/YOLOv8Unity) repository, which provides preconfigured scenes.

### 3. Scene Setup

In Unity, the `Scenes/Segmentation` scene is used. The main camera (`Main Camera`) includes a `Segmentator` component, which handles model inference and result visualization. The ONNX file is assigned to the `Model File` field of the `Segmentator` component.

### 4. Camera Input Processing

The `Segmentator` component captures the camera texture, converts it into a tensor, and performs inference using the ONNX model. Results (bounding boxes, scores, classes, and masks) are processed in the `Update()` method, filtering detections with a confidence threshold (e.g., `score > 0.5`).

### 5. Augmented Reality Visualization

Currently, processing camera input and visualizing results are sufficient. ARFoundation could be used to create AR anchors, but for dynamic objects, this may introduce complexity. Instead, results are overlaid directly on the camera view.

### 6. Optimization for Mobile Devices

To ensure smooth performance on mobile devices, optimizations include:

- **Quantization**: Reducing the model to 8-bit (INT8) or FP16.
- **Input Resolution**: Using 320x320 or 256x256 images instead of 640x640.
- **Hardware Delegates**: Leveraging GPU or NNAPI on Android to accelerate inference.

If import errors occur (e.g., `OnnxImportException`), re-export the model with `opset=9` and update Barracuda to version 1.2.0 or higher.

## Steps to Train the Model

Training a custom YOLOv8n-seg model for object detection requires a structured process. The steps are:

### 1. Data Collection

Collect a dataset of images or videos, ensuring:

- Variety of objects.
- Different lighting conditions (day, night, artificial light).
- Varied angles and perspectives (front, side, top).

Relevant datasets, such as those available in the Ultralytics documentation ([Ultralytics Docs](https://docs.ultralytics.com/)), can be used.

### 2. Data Labeling

Images are annotated with:

- **Bounding Boxes**: Rectangles enclosing each object.
- **Segmentation Masks**: Polygons outlining the exact pixels of each object.

Tools like Roboflow ([Roboflow](https://roboflow.com/)) or LabelImg are ideal for this. Roboflow also supports data augmentation (rotations, brightness changes) to improve model robustness.

### 3. Model Selection

YOLOv8n-seg is chosen for its segmentation capabilities and efficiency on mobile devices. The "nano" variant (YOLOv8n) is the lightest in the YOLOv8 family, ideal for resource-constrained devices.

### 4. Training

Training is performed using the Ultralytics YOLO script in a GPU-enabled environment like Google Colab or Kaggle. An example command is:

```bash
yolo train model=yolov8n-seg.pt data=dataset.yaml epochs=100 imgsz=640
```

- `dataset.yaml`: Defines paths to training and validation images and classes.
- `epochs`: Number of iterations (100 is a common starting point).
- `imgsz`: Input image size (640x640 is standard).

### 5. Evaluation

The trained model is evaluated on a validation set, analyzing metrics such as:

- **Precision**: Percentage of correct detections.
- **Recall**: Percentage of objects detected out of the total.
- **mAP (mean Average Precision)**: Standard metric for evaluating detection and segmentation quality.

### 6. Optimization

If performance is unsatisfactory, adjust:

- **Hyperparameters**: Learning rate, batch size.
- **Data Augmentation**: Additional transformations (flips, color changes).
- **Transfer Learning**: Use a pretrained model as a base.

### 7. Export

Once optimized, the model is exported to ONNX with:

```bash
yolo export model=runs/segment/train/weights/best.pt format=onnx imgsz=640 opset=9
```

The exported model is ready for Unity integration, with further optimizations like quantization for mobile devices.

## Why Use YOLOv8n on Mobile Devices?

YOLOv8n is the lightest variant of the YOLOv8 family, designed to balance speed and accuracy on resource-constrained devices like tablets. According to Ultralytics documentation ([Ultralytics Android App](https://docs.ultralytics.com/hub/app/android/)), its efficiency stems from:

- **Quantization**: YOLOv8n models are quantized to FP16 (16-bit floating point) or INT8 (8-bit integer), reducing model size and computation needs for fast real-time inference.
- **Optimized Architecture**: YOLOv8n uses an anchor-free architecture, simplifying processing and improving efficiency compared to heavier models like YOLOv8x.
- **Hardware Delegate Support**: On Android, YOLOv8n leverages NNAPI or GPU delegates for faster inference, depending on the device hardware.

For example, the Ultralytics Android app achieves up to 30 frames per second on modern devices, demonstrating its suitability for mobile applications ([Ultralytics HUB App](https://docs.ultralytics.com/hub/app/)).

### Comparison of YOLOv8 Models

| Model   | Parameters (millions) | mAP@50:95 | Mobile Speed (ms) |
|---------|-----------------------|-----------|-------------------|
| YOLOv8n | 3.2                   | 37.3      | ~10-15           |
| YOLOv8s | 11.2                  | 44.9      | ~20-25           |
| YOLOv8m | 25.9                  | 50.2      | ~30-40           |

*Note*: Inference times are approximate and hardware-dependent. YOLOv8n is significantly faster, ideal for mobile devices.

### Pretrained YOLOv8 Models for Object Detection

Pretrained YOLOv8 models for object detection can serve as a starting point or reference:

<https://universe.roboflow.com/search?q=fish+model%3Ayolov8>

Example of a bounding box-only detection model:
<https://github.com/Hanhanhannnnnnnnnn/YOLOv8n-DDSW-An-efficient-fish-target-detection-network-for-dense-underwater-scenes>

## What is Segmentation and Why is it Valuable?

Segmentation in computer vision involves dividing an image into regions corresponding to specific objects, assigning each pixel a label (e.g., "object" or "background"). YOLOv8n-seg not only detects object locations with bounding boxes but also generates masks outlining the exact pixels of each object.

### Benefits of Segmentation

1. **Accurate Detection**
   - Segmentation masks enable precise object identification, even in complex scenarios with overlapping or partially obscured objects.
   - This improves system robustness compared to bounding box-only detection.

2. **Enhanced AR Effects**
   - Masks allow for precise visual effects applied to objects, such as:
     - Highlighting an object with a specific color.
     - Adding textures or animations that follow the object’s shape.
     - Displaying interactive information (e.g., object details) when tapping an object on the screen.
   - These effects are more accurate and visually appealing than bounding boxes, which only frame objects without capturing their exact shape.

3. **User Interaction**
   - In an AR application, users can interact with specific objects (e.g., tapping an object for more information), enhancing educational or entertainment experiences.

### Example Application

Imagine a user pointing a tablet at a scene and seeing an object highlighted in green with its name and details displayed nearby. Tapping the object triggers an animation or additional information. These effects, enabled by segmentation, make the application more immersive and engaging than simple bounding box detection.

### Detected Issues

During integration and testing of YOLOv8n-seg in Unity, common issues include:

- **Slow Performance**: On mobile devices, inference may be slower than expected. To address this, the `Segmentator.cs` script includes FPS configuration to adjust the camera update and inference frequency, balancing processing load for a smooth experience. Additionally, reducing the ONNX model resolution to 320x320 or 256x256 is recommended, achieved during conversion with:
  ```bash
  yolo export model=yolov8n-seg.pt format=onnx imgsz=320 opset=9
  ```
  These adjustments should be tested on the target device but do not require retraining the model, only modifying the ONNX resolution.

- **Square Texture Requirement**: YOLOv8n-seg requires square input textures (e.g., 640x640). Non-square textures may cause issues. One solution is to add black bands to the camera texture to make it square, though this reduces efficiency by processing unused areas. Alternatively, this can be leveraged as a feature, creating a "periscope" effect where detected objects are listed on the sides.

## Conclusion

Object detection using YOLOv8n-seg in Unity is feasible for mobile applications. Integrating the model in Unity enables real-time object detection and segmentation, with overlaid information and visual effects. The training process, while requiring effort in data collection and labeling, is manageable. YOLOv8n ensures optimal performance on mobile devices due to its quantization and lightweight architecture. Segmentation enhances functionality, enabling more precise AR effects and immersive user experiences.

## Key References

- [Ultralytics Android App: Real-time Object Detection with YOLO](https://docs.ultralytics.com/hub/app/android/)
- [Ultralytics HUB App: YOLO Models on Mobile Devices](https://docs.ultralytics.com/hub/app/)
- [Ultralytics YOLO Docs: Model Export with YOLO](https://docs.ultralytics.com/modes/export/)
- [Ultralytics YOLO Docs: Home](https://docs.ultralytics.com/)
- [GitHub - ultralytics/ultralytics: YOLO11 Models](https://github.com/ultralytics/ultralytics)
- [YOLOv8Unity: Integration of YOLOv8 in Unity](https://github.com/wojciechp6/YOLOv8Unity)

## How to Run the Unity Project

1. **Install Unity**: Ensure Unity Hub and Unity Editor version 2022.3.20f1 or later are installed.
2. **Clone the Repository**: Clone the GitHub repository to your local machine:
   ```bash
   git clone https://github.com/alexwing/yolov8n-segv8Unity.git
   ```
3. **Open the Project in Unity**: In Unity Hub, select "Add" to include the cloned project folder, then open it.
4. **Dependencies**: Dependencies are automatically installed upon opening the project. Check the Unity Console for errors if issues arise.
5. **ONNX Model**: Pretrained ONNX models are available in the `Models` folder.
6. **Scene Setup**: Open the `Scenes/Segmentation` scene in Unity. Ensure the ONNX model is assigned to the `Segmentator` component.
7. **Run the Project**: Verify the camera is configured and the device is connected. Click "Play" in Unity to run the project. The camera should process images and display real-time object detection results.
8. **Performance Adjustments**: For performance issues, adjust the ONNX model resolution to 320x320 or 256x256 in `Segmentator.cs`. You can also tweak the camera update and inference frequency for better performance.

An APK release is available in the repository for Android devices. Ensure "Install from Unknown Sources" is enabled on your device to install it.

Example videos of the application:

<https://youtu.be/W-A5ViO9d4c>  
<https://youtu.be/0wmiM0OCrTk>  
<https://youtu.be/9NydC1UlFx4>

The YOLOv8n-seg model is trained to detect a variety of objects. Below is a table of detectable objects:

| No. | Object              | No. | Object              | No. | Object              |
|-----|---------------------|-----|---------------------|-----|---------------------|
| 1   | person              | 28  | tie                 | 55  | donut               |
| 2   | bicycle             | 29  | suitcase            | 56  | cake                |
| 3   | car                 | 30  | frisbee             | 57  | chair               |
| 4   | motorcycle          | 31  | skis                | 58  | sofa                |
| 5   | airplane            | 32  | snowboard           | 59  | potted plant        |
| 6   | bus                 | 33  | sports ball         | 60  | bed                 |
| 7   | train               | 34  | kite                | 61  | dining table        |
| 8   | truck               | 35  | baseball bat        | 62  | toilet              |
| 9   | boat                | 36  | baseball glove      | 63  | tv                  |
| 10  | traffic light       | 37  | skateboard          | 64  | laptop              |
| 11  | fire hydrant        | 38  | surfboard           | 65  | mouse               |
| 12  | stop sign           | 39  | tennis racket       | 66  | remote              |
| 13  | parking meter       | 40  | bottle              | 67  | keyboard            |
| 14  | bench               | 41  | wine glass          | 68  | cell phone          |
| 15  | bird                | 42  | cup                 | 69  | microwave           |
| 16  | cat                 | 43  | fork                | 70  | oven                |
| 17  | dog                 | 44  | knife               | 71  | toaster             |
| 18  | horse               | 45  | spoon               | 72  | sink                |
| 19  | sheep               | 46  | bowl                | 73  | refrigerator        |
| 20  | cow                 | 47  | banana              | 74  | book                |
| 21  | elephant            | 48  | apple               | 75  | clock               |
| 22  | bear                | 49  | sandwich            | 76  | vase                |
| 23  | zebra               | 50  | orange              | 77  | scissors            |
| 24  | giraffe             | 51  | broccoli            | 78  | teddy bear          |
| 25  | backpack            | 52  | carrot              | 79  | hair drier          |
| 26  | umbrella            | 53  | hot dog             | 80  | toothbrush          |
| 27  | handbag             | 54  | pizza               |     |                     |