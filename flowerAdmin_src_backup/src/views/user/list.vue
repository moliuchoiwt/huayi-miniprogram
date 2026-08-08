<template>
  <div class="table-box" v-loading="isLoading">
    <ProTable ref="proTable" :columns="columns" :request-api="sysUserInfoApi.list">
      <template #avatar="scope">
        <el-image
          :src="baseImgUrl(scope.row.avatar)"
          fit="contain"
          class="imgUrl"
          :preview-src-list="[baseImgUrl(scope.row.avatar)]"
          preview-teleported
        ></el-image>
      </template>

      <template #time="scope">
        <div>创建时间: {{ scope.row.createTime }}</div>
        <div>修改时间: {{ scope.row.updateTime }}</div>
      </template>
      <template #operation="scope">
        <!-- <el-button type="primary" link :icon="View" @click="dialogTeamOpen({ ...scope.row })"> 查看团队 </el-button> -->
        <el-button type="warning" link :icon="EditPen" @click="operate('编辑', { ...scope.row })"> 编辑 </el-button>
        <el-button type="danger" link :icon="Delete" @click="operate('删除', scope.row)"> 删除 </el-button>
      </template>
    </ProTable>
    <Dialog ref="DialogRef"></Dialog>
    <DialogTeam ref="DialogTeamRef"></DialogTeam>
  </div>
</template>

<script setup lang="ts" name="userList">
import { ref } from "vue";
import { ColumnProps } from "@/components/ProTable/interface";
import { Delete, EditPen } from "@element-plus/icons-vue";
import ProTable from "@/components/ProTable/index.vue";
import { sysUserInfoApi } from "@/api/api";
import { useHandleData } from "@/hooks/useHandleData";
import Dialog from "./components/Dialog.vue";
import DialogTeam from "./components/DialogTeam.vue";
import { baseImgUrl } from "@/utils";

const proTable = ref();
const DialogRef = ref<InstanceType<typeof Dialog> | null>(null);
const DialogTeamRef = ref<InstanceType<typeof DialogTeam> | null>(null);
let isLoading = ref<boolean>(true);

// 获取数据
const getList = async () => {
  isLoading.value = false;
};
getList();
const columns = ref<ColumnProps[]>([
  { prop: "queryName", label: "搜索", isShow: false, isSetting: false, search: { el: "input", key: "queryName", label: "搜索" } },
  { prop: "Id", label: "ID", minWidth: 100, fixed: "left" },
  { prop: "avatar", label: "头像", minWidth: 150 },
  { prop: "nickName", label: "名称", minWidth: 150 },
  { prop: "mobile", label: "手机号", minWidth: 150 },
  { prop: "time", label: "操作时间", minWidth: 250 },
  { prop: "operation", label: "操作", minWidth: 250, fixed: "right" }
]);

const initRow = { status: 0 };
const operate = (title: string, row: any = { ...initRow }) => {
  if (title == "删除") {
    useHandleData(sysUserInfoApi.delete, { ids: [row.Id] }, `确认删除用户 ${row.nickName}`).then(() => {
      proTable.value?.getTableList();
    });
    return;
  }
  const params = {
    title: title + (title == "编辑" ? row.nickName : ""),
    row: { ...row },
    api: sysUserInfoApi.operation,
    getTableList: proTable.value?.getTableList
  };
  DialogRef.value?.acceptParams(params);
};
// const dialogTeamOpen = (row: any) => {
//   const params = {
//     row: { ...row },
//     getTableList: proTable.value?.getTableList
//   };
//   DialogTeamRef.value?.acceptParams(params);
// };
</script>
