<template>
  <div class="table-box">
    <ProTable ref="proTable" :columns="columns" :request-api="sysUserApi.list">
      <!-- 表格 header 按钮 -->
      <template #tableHeader>
        <el-button type="primary" :icon="CirclePlus" @click="operate('新增')">新增账号 </el-button>
      </template>
      <!-- 账号操作 -->
      <template #avatar="scope">
        <el-image
          :src="baseImgUrl(scope.row.avatar)"
          class="avatarImg"
          :preview-src-list="[baseImgUrl(scope.row.avatar)]"
          preview-teleported
        ></el-image>
      </template>
      <template #roleId="scope">
        <div v-for="item in roleList" :key="item.Id" v-show="item.Id == scope.row.roleId">
          <el-tag>{{ item.roleName }}</el-tag>
        </div>
      </template>
      <template #time="scope">
        <div>创建时间: {{ scope.row.createTime }}</div>
        <div>修改时间: {{ scope.row.updateTime }}</div>
        <div>最后登录时间: {{ scope.row.lastLoginTime }}</div>
      </template>
      <template #operation="scope">
        <el-button type="warning" link :icon="EditPen" @click="operate('编辑', { ...scope.row })"> 编辑 </el-button>
        <el-button type="danger" link :disabled="scope.row.Id == 1" :icon="Delete" @click="operate('删除', scope.row)">
          删除
        </el-button>
      </template>
    </ProTable>
    <Dialog ref="DialogRef"></Dialog>
  </div>
</template>

<script setup lang="ts" name="accountManage">
import { ref } from "vue";
import { ColumnProps } from "@/components/ProTable/interface";
import { Delete, EditPen, CirclePlus } from "@element-plus/icons-vue";
import ProTable from "@/components/ProTable/index.vue";
import { sysRoleApi, sysUserApi } from "@/api/api";
import { useHandleData } from "@/hooks/useHandleData";
import Dialog from "./components/Dialog.vue";
import { baseImgUrl } from "@/utils";

const proTable = ref();
const DialogRef = ref<InstanceType<typeof Dialog> | null>(null);

let roleList = ref<any[]>([]);
(async () => {
  const { data }: any = await sysRoleApi.list({ pageNum: 1, pageSize: 100 });
  roleList.value = data.items;
})();

// 表格配置项
const columns: ColumnProps[] = [
  { prop: "queryName", label: "搜索", isShow: false, isSetting: false, search: { el: "input", key: "queryName", label: "搜索" } },
  { prop: "Id", label: "ID", width: 100, fixed: "left" },
  { prop: "avatar", label: "头像", width: 150 },
  { prop: "nickName", label: "名称", width: 150 },
  { prop: "phone", label: "手机号", width: 150 },
  { prop: "roleId", label: "角色", width: 150 },
  { prop: "time", label: "操作时间", width: 250 },
  { prop: "operation", label: "操作", width: 200, fixed: "right" }
];
const initRow = { accountStatus: 0 };
const operate = (title: string, row: any = { ...initRow }) => {
  if (title == "删除") {
    useHandleData(sysUserApi.delete, { ids: [row.Id] }, `确认删除账号 ${row.nickName}`).then(() => {
      proTable.value?.getTableList();
    });
    return;
  }
  const params = {
    title: title + (title == "编辑" ? row.nickName : ""),
    row: { ...row },
    api: sysUserApi.operation,
    getTableList: proTable.value?.getTableList,
    other: [roleList.value]
  };
  DialogRef.value?.acceptParams(params);
};
</script>
<style lang="scss" scoped>
.avatarImg {
  width: 60px;
  height: 60px;
  border-radius: 5px;
}
</style>
