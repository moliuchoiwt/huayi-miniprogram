<template>
    <view>
        <view class="form">
            <up-form labelPosition="top" label-width="700rpx">
                <up-form-item label="花店名称" required>
                    <up-input v-model="query.name" placeholder="请填写花店名称"></up-input>
                </up-form-item>
                <up-form-item label="营业执照照片" required>
                    <view style="display: flex;align-items: center;">
                        <up-upload :fileList="fileList1" @afterRead="afterRead" @delete="deletePic" :name="1" multiple
                            :maxCount="1" width="200rpx" height="200rpx" previewFullImage>
                            <view class="uploadBox">
                                <image src="/static/icon16.png"></image>
                                <view class="text">上传照片</view>
                            </view>
                        </up-upload>
                        <view style="font-size: 20rpx;color: #999999;margin-left: 24rpx;">
                            新办理的营业执照，因国家市场监督管理总局信息更新有延迟，建议在办理成功后等待至少14个工作日后再进入入驻
                        </view>
                    </view>
                </up-form-item>
                <up-form-item label="所在地区" required>
                    <view
                        style="display: flex;align-items: center;justify-content: space-between;width: 100%;padding: 12rpx 0;">
                        <picker mode="region" @change="bindTimeChange">
                            <up-input placeholder="请选择省市区" type="text" readonly v-model="addressName" border="none" />
                        </picker>
                        <view><up-icon name="arrow-right" color="#969696" size="20"></up-icon></view>
                    </view>
                </up-form-item>
                <up-form-item label="详细地址" required>
                    <up-textarea v-model="query.address" placeholder="请选择详细地址具体到街道门牌号"></up-textarea>
                </up-form-item>
                <up-form-item label="联系人姓名" required>
                    <up-input v-model="query.realName" placeholder="请填写联系人姓名"></up-input>
                </up-form-item>
                <up-form-item label="联系人电话" required>
                    <up-input v-model="query.mobile" type="number" placeholder="请填写联系人电话"></up-input>
                </up-form-item>
            </up-form>
        </view>
        <view class="btns">
            <view class="btn" @click="submit">{{ btnText }}</view>
        </view>
    </view>
</template>

<script>
import {
    test
} from '@/uni_modules/uview-plus';
export default {
    data() {
        return {
            query: {
                name: '',
                businessImg: '',
                province: '',
                city: '',
                area: '',
                address: '',
                realName: '',
                mobile: '',
            },
            fileList1: [],
        }
    },
    computed: {
        addressName() {
            let item = this.query
            if (!item.province) return ''
            return item.province + ' ' + item.city + ' ' + item.area
        },
        disabled() {
            return this.storeUser.user.shopAuditStatus != 1
        },
        btnText() {
            if (this.storeUser.user.shopAuditStatus == -1) {
                return '申请入驻'
            } else {
                return ['审核中', '已通过审核', '被驳回重新提交'][this.storeUser.user.shopAuditStatus]
            }
        },
    },
    async onLoad() {
        await this.storeUser.updateUser()
        if (this.storeUser.user.shopAuditStatus == -1) {
            this.query.province = this.storeUser.user.province
            this.query.city = this.storeUser.user.city
            this.query.area = this.storeUser.user.area
            this.query.address = this.storeUser.user.address
            this.query.realName = this.storeUser.user.nickName
            this.query.mobile = this.storeUser.user.mobile
        } else {
            this.request('User/GetShop').then(res => {
                if (res.code == 200) {
                    this.query = res.data.shop
                    this.fileList1 = [{
                        url: this.query.businessImg,
                    }]
                }
            })
        }
    },
    methods: {
        chooseAddress() {
            return new Promise((resolve, reject) => {
                uni.chooseAddress({
                    success: (res) => {
                        resolve(res)
                    },
                    fail: (err) => {
                        reject(err)
                    }
                })
            })
        },
        bindTimeChange(e) {
            this.query.province = e.detail.value[0]
            this.query.city = e.detail.value[1]
            this.query.area = e.detail.value[2]
        },
        // 新增图片
        async afterRead(event) {
            let lists = [].concat(event.file)
            let fileListLen = this[`fileList${event.name}`].length
            lists.map((item) => {
                this[`fileList${event.name}`].push({
                    ...item,
                    status: 'uploading',
                    message: '上传中'
                })
            })
            for (let i = 0; i < lists.length; i++) {
                const result = await this.uploadFile(lists[i].url)
                let item = this[`fileList${event.name}`][fileListLen]
                this[`fileList${event.name}`].splice(fileListLen, 1, Object.assign(item, {
                    status: 'success',
                    message: '',
                    url: result
                }))
                switch (event.name) {
                    case 1:
                        this.query.businessImg = result
                        break;
                }
                fileListLen++
            }
        },
        // 删除图片
        deletePic(event) {
            this[`fileList${event.name}`].splice(event.index, 1)
            switch (event.name) {
                case 1:
                    this.query.businessImg = ""
                    break;
            }
        },
        // 提交
        submit() {
            if (this.storeUser.user.shopAuditStatus == 1) {
                return
            }
            let rules = [
                ['name', '请填写花店名称'],
                ['businessImg', '请上传营业执照照片'],
                ['province', '请选择地址'],
                ['address', '请选择详细地址'],
                ['realName', '请填写联系人姓名'],
                ['mobile', '请填写联系人电话'],
            ]
            for (let i = 0; i < rules.length; i++) {
                let item = rules[i]
                if (!this.query[item[0]]) {
                    this.toast(item[1])
                    return
                }
            }
            if (!test.mobile(this.query.mobile)) {
                this.toast('手机号错误')
                return
            }
            this.request('Shop/SubmitShop', this.query).then(res => {
                this.toast('操作成功')
                setTimeout(() => {
                    this.storeUser.updateUser()
                    this.back()
                }, 400)
            })
        },
    }
}
</script>

<style lang="scss" scoped>
.btns {
    padding: 15rpx 32rpx;
    width: 100%;
    position: fixed;
	z-index: 999;
    left: 0;
    bottom: 0;
    box-shadow: 0rpx 0rpx 16rpx 1rpx rgba(12, 106, 106, 0.1);
    background: #FFFFFF;

    .btn { 
        width: 100%;
        text-align: center;
        padding: 20rpx 0;
        font-size: 28rpx;
        color: #FFFFFF;
        background: #8C4FFF;
        border-radius: 16rpx;
    }
}

.form {
    background: white;
    padding: 0 32rpx;
    width: 686rpx;
    margin: 32rpx;
    border-radius: 16rpx;
}

.item {
    width: 100%;
    display: flex;
    justify-content: space-between;
    padding-right: 32rpx;
}
</style>