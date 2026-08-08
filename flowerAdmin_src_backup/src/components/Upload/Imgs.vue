<template>
  <div class="upload-box">
    <el-upload
      v-model:file-list="_fileList"
      action="#"
      list-type="picture-card"
      :class="['upload', self_disabled ? 'disabled' : '', drag ? 'no-border' : '']"
      :multiple="true"
      :disabled="self_disabled"
      :limit="limit"
      :http-request="handleHttpUpload"
      :before-upload="beforeUpload"
      :on-exceed="handleExceed"
      :on-success="uploadSuccess"
      :on-error="uploadError"
      :drag="drag"
      :accept="fileType.join(',')"
    >
      <div class="upload-empty">
        <slot name="empty">
          <el-icon><Plus /></el-icon>
        </slot>
      </div>
      <template #file="{ file }">
        <div
          class="upload-item"
          draggable="true"
          :class="{ dragging: draggedItem?.uid === file.uid }"
          @dragstart="handleDragStart($event, file)"
          @dragover.prevent="handleDragOver($event, file)"
          @dragenter.prevent="handleDragEnter($event, file)"
          @dragleave.prevent="handleDragLeave"
          @dragend="handleDragEnd"
          @drop.prevent="handleDrop()"
        >
          <img :src="baseImgUrl(file.url)" class="upload-image" />
          <div class="upload-handle" @click.stop>
            <div class="handle-icon" @click="handlePictureCardPreview(file)">
              <el-icon><ZoomIn /></el-icon>
              <span>查看</span>
            </div>
            <div v-if="!self_disabled" class="handle-icon" @click="handleRemove(file)">
              <el-icon><Delete /></el-icon>
              <span>删除</span>
            </div>
          </div>
        </div>
      </template>
    </el-upload>
    <div class="el-upload__tip">
      <slot name="tip"></slot>
    </div>
    <el-image-viewer v-if="imgViewVisible" :url-list="[baseImgUrl(viewImageUrl)]" @close="imgViewVisible = false" />
  </div>
</template>

<script setup lang="ts" name="UploadImgs">
import { ref, computed, inject, watch } from "vue";
import { Plus } from "@element-plus/icons-vue";
import type { UploadProps, UploadFile, UploadUserFile, UploadRequestOptions } from "element-plus";
import { ElNotification, formContextKey, formItemContextKey } from "element-plus";
import { uploadApi } from "@/api/api";
import { baseImgUrl } from "@/utils";

interface UploadFileProps {
  fileList: UploadUserFile[];
  api?: (params: any) => Promise<any>;
  drag?: boolean;
  disabled?: boolean;
  limit?: number;
  fileSize?: number;
  fileType?: string[];
  height?: string;
  width?: string;
  borderRadius?: string;
}

const props = withDefaults(defineProps<UploadFileProps>(), {
  fileList: () => [],
  drag: true,
  disabled: false,
  limit: 5,
  fileSize: 5,
  fileType: () => ["image/jpeg", "image/png", "image/gif"],
  height: "150px",
  width: "150px",
  borderRadius: "8px"
});
// 新增状态
const dragOverItem = ref<UploadUserFile | null>(null);
const isDragging = ref(false);

// 获取 el-form 组件上下文
const formContext = inject(formContextKey, void 0);
// 获取 el-form-item 组件上下文
const formItemContext = inject(formItemContextKey, void 0);
// 判断是否禁用上传和删除
const self_disabled = computed(() => {
  return props.disabled || formContext?.disabled;
});

const _fileList = ref<UploadUserFile[]>(props.fileList);
const draggedItem = ref<UploadUserFile | null>(null);

// 监听 props.fileList 列表默认值改变
watch(
  () => props.fileList,
  (n: UploadUserFile[]) => {
    _fileList.value = n;
  },
  { deep: true }
);
const beforeUpload: UploadProps["beforeUpload"] = rawFile => {
  const imgSize = rawFile.size / 1024 / 1024 < props.fileSize;
  const imgType = props.fileType.includes(rawFile.type);
  if (!imgType)
    ElNotification({
      title: "温馨提示",
      message: "上传图片不符合所需的格式！",
      type: "warning"
    });
  if (!imgSize)
    setTimeout(() => {
      ElNotification({
        title: "温馨提示",
        message: `上传图片大小不能超过 ${props.fileSize}M！`,
        type: "warning"
      });
    }, 0);
  return imgType && imgSize;
};

const handleHttpUpload = async (options: UploadRequestOptions) => {
  let formData = new FormData();
  formData.append("file", options.file);
  try {
    const api = props.api ?? uploadApi.WebUploadFile;
    const { data } = await api(formData);
    options.onSuccess(data);
  } catch (error) {
    options.onError(error as any);
  }
};

const emit = defineEmits<{
  "update:fileList": [value: UploadUserFile[]];
}>();

const uploadSuccess = (response: string | undefined, uploadFile: UploadFile) => {
  if (!response) return;
  uploadFile.url = response;
  emit("update:fileList", _fileList.value);
  formItemContext?.prop && formContext?.validateField([formItemContext.prop as string]);
  ElNotification({
    title: "温馨提示",
    message: "图片上传成功！",
    type: "success"
  });
};

const handleRemove = (file: UploadFile) => {
  _fileList.value = _fileList.value.filter(item => item.uid !== file.uid);
  emit("update:fileList", _fileList.value);
};

const uploadError = () => {
  ElNotification({
    title: "温馨提示",
    message: "图片上传失败，请您重新上传！",
    type: "error"
  });
};

const handleExceed = () => {
  ElNotification({
    title: "温馨提示",
    message: `当前最多只能上传 ${props.limit} 张图片，请移除后上传！`,
    type: "warning"
  });
};

const viewImageUrl = ref("");
const imgViewVisible = ref(false);
const handlePictureCardPreview: UploadProps["onPreview"] = file => {
  viewImageUrl.value = file.url!;
  imgViewVisible.value = true;
};

// 修改后的拖拽方法
const handleDragStart = (e: DragEvent, file: UploadUserFile) => {
  if (self_disabled.value) return;
  draggedItem.value = file;
  isDragging.value = true;
  e.dataTransfer?.setData("text/plain", file.uid as any); // 兼容移动端
};

const handleDragOver = (e: DragEvent, file: UploadUserFile) => {
  if (!isDragging.value || self_disabled.value) return;
  e.preventDefault();
  // 只在鼠标移动超过阈值时才更新目标项
  if (!dragOverItem.value || dragOverItem.value.uid !== file.uid) {
    dragOverItem.value = file;
  }
};

const handleDragEnter = (e: DragEvent, file: UploadUserFile) => {
  if (!isDragging.value || self_disabled.value) return;
  dragOverItem.value = file;
};

const handleDragLeave = () => {
  // 可以添加一些视觉反馈清除
};

const handleDrop = () => {
  if (!isDragging.value || self_disabled.value) return;

  if (draggedItem.value && dragOverItem.value && draggedItem.value.uid !== dragOverItem.value.uid) {
    const oldIndex = _fileList.value.findIndex(item => item.uid === draggedItem.value?.uid);
    const newIndex = _fileList.value.findIndex(item => item.uid === dragOverItem.value?.uid);

    if (oldIndex !== -1 && newIndex !== -1) {
      // 使用splice实现更流畅的移动
      const [removed] = _fileList.value.splice(oldIndex, 1);
      _fileList.value.splice(newIndex, 0, removed);

      emit("update:fileList", [..._fileList.value]);
    }
  }

  handleDragEnd();
};

const handleDragEnd = () => {
  isDragging.value = false;
  draggedItem.value = null;
  dragOverItem.value = null;
};
</script>

<style scoped lang="scss">
.is-error {
  .upload {
    :deep(.el-upload--picture-card),
    :deep(.el-upload-dragger) {
      border: 1px dashed var(--el-color-danger) !important;
      &:hover {
        border-color: var(--el-color-primary) !important;
      }
    }
  }
}
:deep(.disabled) {
  .el-upload--picture-card,
  .el-upload-dragger {
    cursor: not-allowed;
    background: var(--el-disabled-bg-color) !important;
    border: 1px dashed var(--el-border-color-darker);
    &:hover {
      border-color: var(--el-border-color-darker) !important;
    }
  }
}
.upload-box {
  .no-border {
    :deep(.el-upload--picture-card) {
      border: none !important;
    }
  }
  :deep(.upload) {
    .el-upload-dragger {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 100%;
      height: 100%;
      padding: 0;
      overflow: hidden;
      border: 1px dashed var(--el-border-color-darker);
      border-radius: v-bind(borderRadius);
      &:hover {
        border: 1px dashed var(--el-color-primary);
      }
    }
    .el-upload-dragger.is-dragover {
      background-color: var(--el-color-primary-light-9);
      border: 2px dashed var(--el-color-primary) !important;
    }
    .el-upload-list__item,
    .el-upload--picture-card {
      width: v-bind(width);
      height: v-bind(height);
      background-color: transparent;
      border-radius: v-bind(borderRadius);
    }
    .upload-item {
      position: relative;
      width: 100%;
      height: 100%;
      cursor: move;
      transition: transform 0.2s ease;
      &.dragging {
        opacity: 0.5;
        transform: scale(0.95);
      }
      &.drag-over {
        background-color: var(--el-color-primary-light-9);
        border: 2px dashed var(--el-color-primary);
      }
    }
    .upload-image {
      width: 100%;
      height: 100%;
      object-fit: contain;
    }
    .upload-handle {
      position: absolute;
      top: 0;
      right: 0;
      box-sizing: border-box;
      display: flex;
      align-items: center;
      justify-content: center;
      width: 100%;
      height: 100%;
      cursor: pointer;
      background: rgb(0 0 0 / 60%);
      opacity: 0;
      transition: var(--el-transition-duration-fast);
      .handle-icon {
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        padding: 0 6%;
        color: aliceblue;
        .el-icon {
          margin-bottom: 15%;
          font-size: 140%;
        }
        span {
          font-size: 100%;
        }
      }
    }
    .el-upload-list__item {
      &:hover {
        .upload-handle {
          opacity: 1;
        }
      }
    }
    .upload-empty {
      display: flex;
      flex-direction: column;
      align-items: center;
      font-size: 12px;
      line-height: 30px;
      color: var(--el-color-info);
      .el-icon {
        font-size: 28px;
        color: var(--el-text-color-secondary);
      }
    }
  }
  .el-upload__tip {
    line-height: 15px;
    text-align: center;
  }
}
</style>
