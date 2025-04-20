from ultralytics import YOLO

model = YOLO("yolov8n-detect-fish.pt")
# model.export(format="onnx", opset=9)
model.export(format="onnx", opset=9, imgsz=[640, 640])